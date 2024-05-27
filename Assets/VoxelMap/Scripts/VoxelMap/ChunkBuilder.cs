using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class ChunkBuilder{

    [Serializable]
    public struct PartInfo{
        public string partName;
        public  Matrix4x4 localToWorldMatrix;
		public PartInfo(string name, Matrix4x4 localToWorldMatrix){
			partName = name; this.localToWorldMatrix = localToWorldMatrix;
		}
    }
	
	[Serializable]
	public struct MeshData{
        public Mesh mesh;
        public Material[] materials;
    }

	//MARK: Cell building info.
	// I live in Spain, but the S is silent.
	// ("Part name", yrotation * zscale) <- Valores negativos indican que la pieza está en espejo, 4 = sin rotación. 
    private static readonly (string, short)[][] cellBuildingInfo = {

	/*	000	00000000	*/	new (string, short)[]{ },
	/*	001	00000001	*/	new (string, short)[]{ ("1.01",4)},					
	/*	002	00000010	*/  new (string, short)[]{ ("1.01",3)},					
	/*	003	00000011	*/	new (string, short)[]{ ("2.01",4)},
	/*	004	00000100	*/  new (string, short)[]{ ("1.01",2)},					
	/*	005	00000101	*/	new (string, short)[]{ ("1.01",4), ("1.01",2)},
	/*	006	00000110	*/	new (string, short)[]{ ("2.01",3)},
	/*	007	00000111	*/	new (string, short)[]{ ("3.01",3)},
	/*	008	00001000	*/  new (string, short)[]{ ("1.01",1)},					
	/*	009	00001001	*/	new (string, short)[]{ ("2.01",1)},
	/*	010	00001010	*/	new (string, short)[]{ ("1.01",3), ("1.01",1)},
	/*	011	00001011	*/	new (string, short)[]{ ("3.01",4)},
	/*	012	00001100	*/	new (string, short)[]{ ("2.01",2)},
	/*	013	00001101	*/	new (string, short)[]{ ("3.01",1)},
	/*	014	00001110	*/	new (string, short)[]{ ("3.01",2)},
	/*	015	00001111	*/	new (string, short)[]{ ("4.01",4)},
	/*	016	00010000	*/  new (string, short)[]{ ("1.02",4)},					
	/*	017	00010001	*/	new (string, short)[]{ ("2.02",4)},
	/*	018	00010010	*/	new (string, short)[]{ ("1.01",3), ("1.02",4)},
	/*	019	00010011	*/	new (string, short)[]{ ("3.02",4)},
	/*	020	00010100	*/	new (string, short)[]{ ("1.01",2), ("1.02",4)},
	/*	021	00010101	*/	new (string, short)[]{ ("2.02",4),("1.01",2)},
	/*	022	00010110	*/	new (string, short)[]{ ("2.01",3), ("1.02",4)},
	/*	023	00010111	*/	new (string, short)[]{ ("4.02",4)},
	/*	024	00011000	*/	new (string, short)[]{ ("1.01",1), ("1.02",4)},
	/*	025	00011001	*/	new (string, short)[]{ ("3.02",-1)},
	/*	026	00011010	*/	new (string, short)[]{ ("1.01",3),("1.01",1),("1.02",4)},
	/*	027	00011011	*/	new (string, short)[]{ ("4.06",4)},
	/*	028	00011100	*/	new (string, short)[]{ ("2.01",2), ("1.02",4)},
	/*	029	00011101	*/	new (string, short)[]{ ("4.02",-1)},
	/*	030	00011110	*/	new (string, short)[]{ ("3.01",2), ("1.02",4)},
	/*	031	00011111	*/	new (string, short)[]{ ("5.01",4)},
	/*	032	00100000	*/	new (string, short)[]{ ("1.02",3)},					
	/*	033	00100001	*/	new (string, short)[]{ ("1.01",4), ("1.02",3)},
	/*	034	00100010	*/	new (string, short)[]{ ("2.02",3)},
	/*	035	00100011	*/	new (string, short)[]{ ("3.02",-4)},
	/*	036	00100100	*/	new (string, short)[]{ ("1.01",2), ("1.02",3)},
	/*	037	00100101	*/	new (string, short)[]{ ("1.01",2),("1.01",4),("1.02",3)},
	/*	038	00100110	*/	new (string, short)[]{ ("3.02",3)},
	/*	039	00100111	*/	new (string, short)[]{ ("4.06",3)},
	/*	040	00101000	*/	new (string, short)[]{ ("1.01",1), ("1.02",3)},
	/*	041	00101001	*/	new (string, short)[]{ ("2.01",1), ("1.02",3)},
	/*	042	00101010	*/	new (string, short)[]{ ("2.02",3),("1.01",1)},
	/*	043	00101011	*/	new (string, short)[]{ ("4.02",-4)},
	/*	044	00101100	*/	new (string, short)[]{ ("2.01",2), ("1.02",3)},
	/*	045	00101101	*/	new (string, short)[]{ ("3.01",1), ("1.02",3)},
	/*	046	00101110	*/	new (string, short)[]{ ("4.02",3)},
	/*	047	00101111	*/	new (string, short)[]{ ("5.01",3)},
	/*	048	00110000	*/	new (string, short)[]{ ("2.03",4)},
	/*	049	00110001	*/	new (string, short)[]{ ("3.03",4)},
	/*	050	00110010	*/	new (string, short)[]{ ("3.03",-4)},
	/*	051	00110011	*/	new (string, short)[]{ ("4.03",4)},
	/*	052	00110100	*/	new (string, short)[]{ ("2.03",4), ("1.01",2)},
	/*	053	00110101	*/	new (string, short)[]{ ("3.03",4), ("1.01",2)},
	/*	054	00110110	*/	new (string, short)[]{ ("4.07",-4)},
	/*	055	00110111	*/	new (string, short)[]{ ("5.02",-4)},
	/*	056	00111000	*/	new (string, short)[]{ ("2.03",4), ("1.01",1)},
	/*	057	00111001	*/	new (string, short)[]{ ("4.07",4)},
	/*	058	00111010	*/	new (string, short)[]{ ("3.03",-4), ("1.01",1)},
	/*	059	00111011	*/	new (string, short)[]{ ("5.02",4)},
	/*	060	00111100	*/	new (string, short)[]{ ("2.03",4), ("2.01",2)},
	/*	061	00111101	*/	new (string, short)[]{ ("5.08",4)},
	/*	062	00111110	*/	new (string, short)[]{ ("5.08",-4)},
	/*	063	00111111	*/	new (string, short)[]{ ("6.01",4)},
	/*	064	01000000	*/	new (string, short)[]{ ("1.02",2)},					
	/*	065	01000001	*/	new (string, short)[]{ ("1.01",4), ("1.02",2)},
	/*	066	01000010	*/	new (string, short)[]{ ("1.01",3), ("1.02",2)},
	/*	067	01000011	*/	new (string, short)[]{ ("2.01",4), ("1.02",2)},
	/*	068	01000100	*/	new (string, short)[]{ ("2.02",2)},
	/*	069	01000101	*/	new (string, short)[]{ ("2.02",2),("1.01",4)},
	/*	070	01000110	*/	new (string, short)[]{ ("3.02",-3)},
	/*	071	01000111	*/	new (string, short)[]{ ("4.02",-3)},
	/*	072	01001000	*/	new (string, short)[]{ ("1.01",1), ("1.02",2)},
	/*	073	01001001	*/	new (string, short)[]{ ("2.01",1), ("1.02",2)},
	/*	074	01001010	*/	new (string, short)[]{ ("1.01",1),("1.01",3),("1.02",2)},
	/*	075	01001011	*/	new (string, short)[]{ ("3.01",4), ("1.02",2)},
	/*	076	01001100	*/	new (string, short)[]{ ("3.02",2)},
	/*	077	01001101	*/	new (string, short)[]{ ("4.02",2)},
	/*	078	01001110	*/	new (string, short)[]{ ("4.06",2)},
	/*	079	01001111	*/	new (string, short)[]{ ("5.01",2)},
	/*	080	01010000	*/	new (string, short)[]{ ("1.02",4), ("1.02",2)},
	/*	081	01010001	*/	new (string, short)[]{ ("2.02",4), ("1.02",2)},
	/*	082	01010010	*/	new (string, short)[]{ ("1.02",2), ("1.02",4),("1.01",3)},
	/*	083	01010011	*/	new (string, short)[]{ ("3.02",4), ("1.02",2)},
	/*	084	01010100	*/	new (string, short)[]{ ("2.02",2), ("1.02",4)},
	/*	085	01010101	*/	new (string, short)[]{ ("2.02",2), ("2.02",4)},
	/*	086	01010110	*/	new (string, short)[]{ ("3.02",-3),("1.02",4)},
	/*	087	01010111	*/	new (string, short)[]{ ("5.05",3)},
	/*	088	01011000	*/	new (string, short)[]{ ("1.02",2),("1.02",4),("1.01",1)},
	/*	089	01011001	*/	new (string, short)[]{ ("3.02",-1), ("1.02",2)},
	/*	090	01011010	*/	new (string, short)[]{ ("1.02",2),("1.02",4),("1.01",1), ("1.01",3)},
	/*	091	01011011	*/	new (string, short)[]{ ("4.06",4), ("1.02",2)},
	/*	092	01011100	*/	new (string, short)[]{ ("3.02",2), ("1.02",4)},
	/*	093	01011101	*/	new (string, short)[]{ ("5.05",1)},
	/*	094	01011110	*/	new (string, short)[]{ ("4.06",2), ("1.02",4)},
	/*	095	01011111	*/	new (string, short)[]{ ("6.04",3)},
	/*	096	01100000	*/	new (string, short)[]{ ("2.03",3)},
	/*	097	01100001	*/	new (string, short)[]{ ("2.03",3), ("1.01",4)},
	/*	098	01100010	*/	new (string, short)[]{ ("3.03",3)},
	/*	099	01100011	*/	new (string, short)[]{ ("4.07",3)},
	/*	100	01100100	*/	new (string, short)[]{ ("3.03",-3)},
	/*	101	01100101	*/	new (string, short)[]{ ("3.03",-3), ("1.01",4)},
	/*	102	01100110	*/	new (string, short)[]{ ("4.03",3)},
	/*	103	01100111	*/	new (string, short)[]{ ("5.02",3)},
	/*	104	01101000	*/	new (string, short)[]{ ("2.03",3), ("1.01",1)},
	/*	105	01101001	*/	new (string, short)[]{ ("2.03",3), ("2.01",1)},
	/*	106	01101010	*/	new (string, short)[]{ ("3.03",3), ("1.01",1)},
	/*	107	01101011	*/	new (string, short)[]{ ("5.08",3)},
	/*	108	01101100	*/	new (string, short)[]{ ("4.07",-3)},
	/*	109	01101101	*/	new (string, short)[]{ ("5.08",-3)},
	/*	110	01101110	*/	new (string, short)[]{ ("5.02",-3)},
	/*	111	01101111	*/	new (string, short)[]{ ("6.01",3)},
	/*	112	01110000	*/	new (string, short)[]{ ("3.04",3)},
	/*	113	01110001	*/	new (string, short)[]{ ("4.08",3)},
	/*	114	01110010	*/	new (string, short)[]{ ("4.04",3)},
	/*	115	01110011	*/	new (string, short)[]{ ("5.03",-4)},
	/*	116	01110100	*/	new (string, short)[]{ ("4.08",-4)},
	/*	117	01110101	*/	new (string, short)[]{ ("5.07",3)},
	/*	118	01110110	*/	new (string, short)[]{ ("5.03",3)},
	/*	119	01110111	*/	new (string, short)[]{ ("6.02",3)},
	/*	120	01111000	*/	new (string, short)[]{ ("3.04",3), ("1.01",1)},
	/*	121	01111001	*/	new (string, short)[]{ ("5.06",-4)},
	/*	122	01111010	*/	new (string, short)[]{ ("4.04",3), ("1.01",1)},
	/*	123	01111011	*/	new (string, short)[]{ ("6.05",3)},
	/*	124	01111100	*/	new (string, short)[]{ ("5.06",3)},
	/*	125	01111101	*/	new (string, short)[]{ ("6.07",3)},
	/*	126	01111110	*/	new (string, short)[]{ ("6.05",-4)},
	/*	127	01111111	*/	new (string, short)[]{ ("7.01",3)},
	/*	128	10000000	*/	new (string, short)[]{ ("1.02",1)},					
	/*	129	10000001	*/	new (string, short)[]{ ("1.01",4), ("1.02",1)},
	/*	130	10000010	*/	new (string, short)[]{ ("1.01",3), ("1.02",1)},
	/*	131	10000011	*/	new (string, short)[]{ ("2.01",4), ("1.02",1)},
	/*	132	10000100	*/	new (string, short)[]{ ("1.01",2), ("1.02",1)},
	/*	133	10000101	*/	new (string, short)[]{ ("1.01",4),("1.01",2),("1.02",1)},
	/*	134	10000110	*/	new (string, short)[]{ ("2.01",3), ("1.02",1)},
	/*	135	10000111	*/	new (string, short)[]{ ("3.01",3), ("1.02",1)},
	/*	136	10001000	*/	new (string, short)[]{ ("2.02",1)},
	/*	137	10001001	*/	new (string, short)[]{ ("3.02",1)},
	/*	138	10001010	*/	new (string, short)[]{ ("2.02",1),("1.01",3)},
	/*	139	10001011	*/	new (string, short)[]{ ("4.02",1)},
	/*	140	10001100	*/	new (string, short)[]{ ("3.02",-2)},
	/*	141	10001101	*/	new (string, short)[]{ ("4.06",1)},
	/*	142	10001110	*/	new (string, short)[]{ ("4.02",-2)},
	/*	143	10001111	*/	new (string, short)[]{ ("5.01",1)},
	/*	144	10010000	*/	new (string, short)[]{ ("2.03",1)},
	/*	145	10010001	*/	new (string, short)[]{ ("3.03",-1)},
	/*	146	10010010	*/	new (string, short)[]{ ("2.03",1), ("1.01",3)},
	/*	147	10010011	*/	new (string, short)[]{ ("4.07",-1)},
	/*	148	10010100	*/	new (string, short)[]{ ("2.03",1), ("1.01",2)},
	/*	149	10010101	*/	new (string, short)[]{ ("3.03",-1), ("1.01",2)},
	/*	150	10010110	*/	new (string, short)[]{ ("2.03",1), ("2.01",3)},
	/*	151	10010111	*/	new (string, short)[]{ ("5.08",-1)},
	/*	152	10011000	*/	new (string, short)[]{ ("3.03",1)},
	/*	153	10011001	*/	new (string, short)[]{ ("4.03",1)},
	/*	154	10011010	*/	new (string, short)[]{ ("3.03",1), ("1.01",3)},
	/*	155	10011011	*/	new (string, short)[]{ ("5.02",-1)},
	/*	156	10011100	*/	new (string, short)[]{ ("4.07",1)},
	/*	157	10011101	*/	new (string, short)[]{ ("5.02",1)},
	/*	158	10011110	*/	new (string, short)[]{ ("5.08",1)},
	/*	159	10011111	*/	new (string, short)[]{ ("6.01",1)},
	/*	160	10100000	*/	new (string, short)[]{ ("1.02",3), ("1.02",1)},
	/*	161	10100001	*/	new (string, short)[]{ ("1.02",1),("1.02",3),("1.01",4)},
	/*	162	10100010	*/	new (string, short)[]{ ("2.02",3), ("1.02",1)},
	/*	163	10100011	*/	new (string, short)[]{ ("3.02",-4), ("1.02",1)},
	/*	164	10100100	*/	new (string, short)[]{ ("1.02",1),("1.02",3),("1.01",2)},
	/*	165	10100101	*/	new (string, short)[]{ ("1.02",1),("1.02",3),("1.01",2), ("1.01",4)},
	/*	166	10100110	*/	new (string, short)[]{ ("3.02",3), ("1.02",1)},
	/*	167	10100111	*/	new (string, short)[]{ ("4.06",3), ("1.02",1)},
	/*	168	10101000	*/	new (string, short)[]{ ("2.02",1), ("1.02",3)},
	/*	169	10101001	*/	new (string, short)[]{ ("3.02",1), ("1.02",3)},
	/*	170	10101010	*/	new (string, short)[]{ ("2.02",1),("2.02",3)},
	/*	171	10101011	*/	new (string, short)[]{ ("5.05",4)},
	/*	172	10101100	*/	new (string, short)[]{ ("3.02",-2), ("1.02",3)},
	/*	173	10101101	*/	new (string, short)[]{ ("4.06",1), ("1.02",3)},
	/*	174	10101110	*/	new (string, short)[]{ ("5.05",2)},
	/*	175	10101111	*/	new (string, short)[]{ ("6.04",4)},
	/*	176	10110000	*/	new (string, short)[]{ ("3.04",4)},
	/*	177	10110001	*/	new (string, short)[]{ ("4.04",4)},
	/*	178	10110010	*/	new (string, short)[]{ ("4.08",-1)},
	/*	179	10110011	*/	new (string, short)[]{ ("5.03",4)},
	/*	180	10110100	*/	new (string, short)[]{ ("3.04",4), ("1.01",2)},
	/*	181	10110101	*/	new (string, short)[]{ ("4.04",4), ("1.01",2)},
	/*	182	10110110	*/	new (string, short)[]{ ("5.06",4)},
	/*	183	10110111	*/	new (string, short)[]{ ("6.05",-1)},
	/*	184	10111000	*/	new (string, short)[]{ ("4.08",4)},
	/*	185	10111001	*/	new (string, short)[]{ ("5.03",-1)},
	/*	186	10111010	*/	new (string, short)[]{ ("5.07",4)},
	/*	187	10111011	*/	new (string, short)[]{ ("6.02",4)},
	/*	188	10111100	*/	new (string, short)[]{ ("5.06",-1)},
	/*	189	10111101	*/	new (string, short)[]{ ("6.05",4)},
	/*	190	10111110	*/	new (string, short)[]{ ("6.07",4)},
	/*	191	10111111	*/	new (string, short)[]{ ("7.01",4)},
	/*	192	11000000	*/	new (string, short)[]{ ("2.03",2)},
	/*	193	11000001	*/	new (string, short)[]{ ("2.03",2), ("1.01",4)},
	/*	194	11000010	*/	new (string, short)[]{ ("2.03",2), ("1.01",3)},
	/*	195	11000011	*/	new (string, short)[]{ ("2.03",2), ("2.01",4)},
	/*	196	11000100	*/	new (string, short)[]{ ("3.03",2)},
	/*	197	11000101	*/	new (string, short)[]{ ("3.03",2), ("1.01",4)},
	/*	198	11000110	*/	new (string, short)[]{ ("4.07",2)},
	/*	199	11000111	*/	new (string, short)[]{ ("5.08",2)},
	/*	200	11001000	*/	new (string, short)[]{ ("3.03",-2)},
	/*	201	11001001	*/	new (string, short)[]{ ("4.07",-2)},
	/*	202	11001010	*/	new (string, short)[]{ ("3.03",-2), ("1.01",3)},
	/*	203	11001011	*/	new (string, short)[]{ ("5.08",-2)},
	/*	204	11001100	*/	new (string, short)[]{ ("4.03",2)},
	/*	205	11001101	*/	new (string, short)[]{ ("5.02",-2)},
	/*	206	11001110	*/	new (string, short)[]{ ("5.02",2)},
	/*	207	11001111	*/	new (string, short)[]{ ("6.01",2)},
	/*	208	11010000	*/	new (string, short)[]{ ("3.04",1)},
	/*	209	11010001	*/	new (string, short)[]{ ("4.08",-2)},
	/*	210	11010010	*/	new (string, short)[]{ ("3.04",1), ("1.01",3)},
	/*	211	11010011	*/	new (string, short)[]{ ("5.06",1)},
	/*	212	11010100	*/	new (string, short)[]{ ("4.08",1)},
	/*	213	11010101	*/	new (string, short)[]{ ("5.07",1)},
	/*	214	11010110	*/	new (string, short)[]{ ("5.06",-2)},
	/*	215	11010111	*/	new (string, short)[]{ ("6.07",1)},
	/*	216	11011000	*/	new (string, short)[]{ ("4.04",1)},
	/*	217	11011001	*/	new (string, short)[]{ ("5.03",1)},
	/*	218	11011010	*/	new (string, short)[]{ ("4.04",1), ("1.01",3)},
	/*	219	11011011	*/	new (string, short)[]{ ("6.05",-2)},
	/*	220	11011100	*/	new (string, short)[]{ ("5.03",-2)},
	/*	221	11011101	*/	new (string, short)[]{ ("6.02",1)},
	/*	222	11011110	*/	new (string, short)[]{ ("6.05",1)},
	/*	223	11011111	*/	new (string, short)[]{ ("7.01",1)},
	/*	224	11100000	*/	new (string, short)[]{ ("3.04",2)},
	/*	225	11100001	*/	new (string, short)[]{ ("3.04",2), ("1.01",4)},
	/*	226	11100010	*/	new (string, short)[]{ ("4.08",2)},
	/*	227	11100011	*/	new (string, short)[]{ ("5.06",-3)},
	/*	228	11100100	*/	new (string, short)[]{ ("4.04",2)},
	/*	229	11100101	*/	new (string, short)[]{ ("4.04",2), ("1.01",4)},
	/*	230	11100110	*/	new (string, short)[]{ ("5.03",-3)},
	/*	231	11100111	*/	new (string, short)[]{ ("6.05",2)},
	/*	232	11101000	*/	new (string, short)[]{ ("4.08",-3)},
	/*	233	11101001	*/	new (string, short)[]{ ("5.06",2)},
	/*	234	11101010	*/	new (string, short)[]{ ("5.07",2)},
	/*	235	11101011	*/	new (string, short)[]{ ("6.07",2)},
	/*	236	11101100	*/	new (string, short)[]{ ("5.03",2)},
	/*	237	11101101	*/	new (string, short)[]{ ("6.05",-3)},
	/*	238	11101110	*/	new (string, short)[]{ ("6.02",2)},
	/*	239	11101111	*/	new (string, short)[]{ ("7.01",2)},
	/*	240	11110000	*/	new (string, short)[]{ ("4.05",4)},
	/*	241	11110001	*/	new (string, short)[]{ ("5.04",4)},
	/*	242	11110010	*/	new (string, short)[]{ ("5.04",3)},
	/*	243	11110011	*/	new (string, short)[]{ ("6.03",4)},
	/*	244	11110100	*/	new (string, short)[]{ ("5.04",2)},
	/*	245	11110101	*/	new (string, short)[]{ ("6.06",3)},
	/*	246	11110110	*/	new (string, short)[]{ ("6.03",3)},
	/*	247	11110111	*/	new (string, short)[]{ ("7.02",3)},
	/*	248	11111000	*/	new (string, short)[]{ ("5.04",1)},
	/*	249	11111001	*/	new (string, short)[]{ ("6.03",1)},
	/*	250	11111010	*/	new (string, short)[]{ ("6.06",4)},
	/*	251	11111011	*/	new (string, short)[]{ ("7.02",4)},
	/*	252	11111100	*/	new (string, short)[]{ ("6.03",2)},
	/*	253	11111101	*/	new (string, short)[]{ ("7.02",1)},
	/*	254	11111110	*/	new (string, short)[]{ ("7.02",2)},
	/*	255	11111111	*/	new (string, short)[]{ }
    };

	//MARK: Methods
    public static PartInfo[] AddCellInfo(byte celltype, Vector3 cellPosition, float voxelSize)
    {
		ArrayList partInfos = new();

		foreach((string name, short transformation) in cellBuildingInfo[celltype]){

			Matrix4x4 transformMatrix;
			if (transformation >0){
				transformMatrix=  Matrix4x4.TRS(
					cellPosition,
					Quaternion.Euler(0, (Mathf.Abs(transformation) % 4) * 90, 0),
					Vector3.one * 100  * voxelSize);
			}else{
				transformMatrix=  Matrix4x4.TRS(
					cellPosition,
					Quaternion.Euler(0, (Mathf.Abs(transformation) % 4) * 90, 0),
					new Vector3(-100,100,100) * voxelSize);
			}

			partInfos.Add(new PartInfo(name, transformMatrix));
		}

        return partInfos.ToArray(typeof(PartInfo)) as PartInfo[];

    }

	public static MeshData BuildChunk(PartInfo[] cellDatas, VoxelMaterial voxelMaterial){

		ArrayList materials = new ArrayList();
        ArrayList combineInstanceArrays = new ArrayList();

		//Setup array of arrays of combineInstances.
        //Probably can be optimized with multithreading.

		foreach(PartInfo cell in cellDatas){
			VoxelMaterial.PartData part = voxelMaterial.basicParts[cell.partName];
			for(int s = 0; s < part.mesh.subMeshCount; s++){
                
                int materialArrayIndex = SearchForMaterial(materials, 
					voxelMaterial.materials[part.materialIndexes[s]].name
					);

                if(materialArrayIndex == -1){
                    materials.Add(voxelMaterial.materials[part.materialIndexes[s]]);
                    materialArrayIndex = materials.Count - 1;

                }

                combineInstanceArrays.Add(new ArrayList());

                CombineInstance combineInstance = new CombineInstance{
                    transform = cell.localToWorldMatrix,
                    subMeshIndex = s,
                    mesh = part.mesh
                };
                (combineInstanceArrays[materialArrayIndex] as ArrayList).Add(combineInstance);

            }
		}

		//Combine meshes by material
        Mesh[] meshes = new Mesh[materials.Count];
        CombineInstance[] combineInstances = new CombineInstance[materials.Count];

        for( int m = 0; m < materials.Count; m++){
            CombineInstance[] combineInstanceArray = 
                (combineInstanceArrays[m] as ArrayList).ToArray(typeof(CombineInstance)) as CombineInstance[];
            
            meshes[m] = new Mesh();
            meshes[m].indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            meshes[m].CombineMeshes(combineInstanceArray,true,true);

            combineInstances[m] = new CombineInstance
            {
                mesh = meshes[m],
                subMeshIndex = 0
            };
        }
		//Combining into one mesh
        Mesh outMesh = new Mesh();
        outMesh.CombineMeshes(combineInstances, false,false);

        //Setting the combined mesh data
        Material[] materialsArray = materials.ToArray(typeof(Material)) as Material[];

		return new MeshData
        {
            materials = materialsArray,
            mesh = outMesh
        };
	}

	
    static int SearchForMaterial(ArrayList materialList, string searchName){

        for(int i = 0; i < materialList.Count; i++){
            if(((Material)materialList[i]).name == searchName)
                return i;
        }

        return -1;
    }
        


}
