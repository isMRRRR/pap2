using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Data.SqlClient;
using DevComponents.AdvTree;


namespace TableSelector
{
    public partial class TreeForm : DevComponents.DotNetBar.Office2007Form
    {
        SqlConnection conn; // sql����
        string tableName; // ����
        string classifyField; // �����ֶ�
        string returnField; // ���ص��ֶ�
        string selectedValue; // ѡ���ֵ
        string oldValue; // ��ֵ
        Hashtable selectedValues = new Hashtable(); // ѡ���ֵ��
        DataTable m_table = null; // ���ݱ�

        string filter = "";  //������

        /// <summary>
        /// ���캯��
        /// </summary>
        /// <param name="conn">sql����</param>
        /// <param name="title">����ı���</param>
        /// <param name="tableName">����</param>
        /// <param name="classifyField">�����ֶ�</param>
        /// <param name="returnField">���ص��ֶ�</param>
        /// <param name="oldValue"></param>
        public TreeForm(SqlConnection conn, string title, string tableName, string classifyField, string returnField, string oldValue)
        {
            this.conn = conn;
            this.tableName = tableName;            
            this.classifyField = classifyField;
            this.returnField = returnField;
            InitializeComponent();
            ReloadTree();
            this.Text = title;
            this.OldValue = oldValue;
        }

        public string OldValue
        {
            get { return oldValue; }
            set 
            { 
                oldValue = value;
                SelectNode(advTree1.Nodes, value);
            }
        }

        private void SelectNode(NodeCollection nodes, string strNodeText)
        {
            foreach (Node node in nodes)
            {
                if (node.Nodes.Count == 0 && node.Tag.ToString() == strNodeText)
                {
                    Node tnode = new Node();
                    node.Nodes.Add(tnode);
                    advTree1.SelectedNode = node;
                    tnode.EnsureVisible();
                    node.Nodes.Clear();
                }
                else
                {
                    SelectNode(node.Nodes, strNodeText);
                }
            }
        }

        /// <summary>
        /// ѡ���ֵ
        /// </summary>
        public string SelectedValue
        {
            get
            {
                return selectedValue;
            }
        }

        /// <summary>
        /// ѡ���ֵ��
        /// </summary>
        public Hashtable SelectedValues
        {
            get
            {
                return selectedValues;
            }
        }

        /// <summary>
        /// ���¼��ؽ�������
        /// </summary>
        private void ReloadTree()
        {      
            if (m_table == null)
            {
                try
                {
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    string sqlString = tableName;
                    m_table = GetDataTable(sqlString);
                }
                catch (SqlException ex)
                {
                    MessageBox.Show("�ڶ�ȡ���ݱ�ʱ����sql[" + tableName + "]�쳣��" + ex.ToString());
                }
                finally
                {
                    if (conn.State == ConnectionState.Open)
                    {
                        conn.Close();
                    }
                }
            }

            advTree1.Nodes.Clear();
            string[] fields = classifyField.Split(new char[]{','});
            DevComponents.AdvTree.NodeCollection nodes;
            DevComponents.AdvTree.Node newNode;
            bool find = false;
            
            foreach(DataRow row in m_table.Rows)
            {
                nodes = advTree1.Nodes;

                //֧�ֹ�����
                if(filter != "")
                {
                    string s = row[fields[fields.Length - 1].ToString()].ToString();
                    if (s.ToLower().IndexOf(filter.ToLower()) == -1 &&
                        Chinese2Spell.GetHeadOfChs(s).ToLower().IndexOf(filter.ToLower()) == -1
                        )
                    {
                        continue;
                    }
                }

                for(int i = 0; i < fields.Length; i++)
                {
                    string field = fields[i];
                    find = false;

                    foreach(DevComponents.AdvTree.Node node in nodes)
                    {
                        if(node.Text == row[field].ToString())
                        {
                            nodes = node.Nodes;
                            find = true;
                            break;
                        }
                    }

                    if(!find)
                    {
                        newNode = new DevComponents.AdvTree.Node();
                        newNode.Text = row[field].ToString();
                        nodes.Add(newNode);                        
                        nodes = newNode.Nodes;

                        if(i == fields.Length - 1)
                        {
                            newNode.Tag = row[returnField].ToString();
                            //if(newNode.Tag.ToString() == oldValue)
                            //{
                            //    selectedNode = newNode;
                            //}
                        }
                    }
                }
            }

            // ѡ�о�ֵ�����������
            //advTree1.SelectedNode = selectedNode;
            //if(selectedNode != null)
            //{
            //    advTree1.SelectedNode.EnsureVisible();
            //}                                    
        }

        /// <summary>
        /// ȷ��ѡ��
        /// </summary>
        /// <param name="sender">�¼�������</param>
        /// <param name="e">�¼�����</param>
        private void buttonX1_Click(object sender, EventArgs e)
        {
            DevComponents.AdvTree.Node selectedNode = advTree1.SelectedNode;

            if(selectedNode != null && selectedNode.Tag != null)
            {
                selectedValue = selectedNode.Tag.ToString();
                DataRow[] rows = m_table.Select(string.Format("[{0}] = '{1}'", returnField, selectedValue));
                if(rows.Length > 0)
                {
                    DataRow row = rows[0];
                    foreach (DataColumn column in m_table.Columns)
                    {
                        selectedValues[column.ColumnName] = row[column.ColumnName];
                    }
                }

                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                MessageBox.Show("��Ч��ѡ��", "����ѡ��", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// ȡ��ѡ��
        /// </summary>
        /// <param name="sender">�¼�������</param>
        /// <param name="e">�¼�����</param>
        private void buttonX2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// ��ȡ���ݱ�
        /// </summary>
        /// <param name="sql">sql���</param>
        /// <returns>���ݱ�</returns>
        private DataTable GetDataTable(string sql)
        {
            SqlDataAdapter adp = new SqlDataAdapter(sql, conn);
            adp.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            DataSet ds = new DataSet();
            adp.Fill(ds);
            System.Data.DataTable tbl = ds.Tables[0];
            return tbl;
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            this.filter = textBoxX1.Text;
            advTree1.Nodes.Clear();
            ReloadTree();
            this.OldValue = this.OldValue;
        }
    }


    public class Chinese2Spell
    {
        #region ���붨��
        private static int[] pyvalue = new int[]
        {
        -20319, -20317, -20304, -20295, -20292, -20283, -20265, -20257, -20242, -20230, 
        -20051, -20036, -20032, -20026, -20002, -19990, -19986, -19982, -19976, -19805, 
        -19784, -19775, -19774, -19763, -19756, -19751, -19746, -19741, -19739, -19728,
        -19725, -19715, -19540, -19531, -19525, -19515, -19500, -19484, -19479, -19467, 
        -19289, -19288, -19281, -19275, -19270, -19263, -19261, -19249, -19243, -19242, 
        -19238, -19235, -19227, -19224, -19218, -19212, -19038, -19023, -19018, -19006, 
        -19003, -18996, -18977, -18961, -18952, -18783, -18774, -18773, -18763, -18756, 
        -18741, -18735, -18731, -18722, -18710, -18697, -18696, -18526, -18518, -18501, 
        -18490, -18478, -18463, -18448, -18447, -18446, -18239, -18237, -18231, -18220, 
        -18211, -18201, -18184, -18183, -18181, -18012, -17997, -17988, -17970, -17964, 
        -17961, -17950, -17947, -17931, -17928, -17922, -17759, -17752, -17733, -17730,
        -17721, -17703, -17701, -17697, -17692, -17683, -17676, -17496, -17487, -17482, 
        -17468, -17454, -17433, -17427, -17417, -17202, -17185, -16983, -16970, -16942, 
        -16915, -16733, -16708, -16706, -16689, -16664, -16657, -16647, -16474, -16470, 
        -16465, -16459, -16452, -16448, -16433, -16429, -16427, -16423, -16419, -16412,
        -16407, -16403, -16401, -16393, -16220, -16216, -16212, -16205, -16202, -16187, 
        -16180, -16171, -16169, -16158, -16155, -15959, -15958, -15944, -15933, -15920, 
        -15915, -15903, -15889, -15878, -15707, -15701, -15681, -15667, -15661, -15659, 
        -15652, -15640, -15631, -15625, -15454, -15448, -15436, -15435, -15419, -15416,
        -15408, -15394, -15385, -15377, -15375, -15369, -15363, -15362, -15183, -15180, 
        -15165, -15158, -15153, -15150, -15149, -15144, -15143, -15141, -15140, -15139, 
        -15128, -15121, -15119, -15117, -15110, -15109, -14941, -14937, -14933, -14930, 
        -14929, -14928, -14926, -14922, -14921, -14914, -14908, -14902, -14894, -14889, 
        -14882, -14873, -14871, -14857, -14678, -14674, -14670, -14668, -14663, -14654, 
        -14645, -14630, -14594, -14429, -14407, -14399, -14384, -14379, -14368, -14355, 
        -14353, -14345, -14170, -14159, -14151, -14149, -14145, -14140, -14137, -14135, 
        -14125, -14123, -14122, -14112, -14109, -14099, -14097, -14094, -14092, -14090,
        -14087, -14083, -13917, -13914, -13910, -13907, -13906, -13905, -13896, -13894, 
        -13878, -13870, -13859, -13847, -13831, -13658, -13611, -13601, -13406, -13404, 
        -13400, -13398, -13395, -13391, -13387, -13383, -13367, -13359, -13356, -13343, 
        -13340, -13329, -13326, -13318, -13147, -13138, -13120, -13107, -13096, -13095, 
        -13091, -13076, -13068, -13063, -13060, -12888, -12875, -12871, -12860, -12858, 
        -12852, -12849, -12838, -12831, -12829, -12812, -12802, -12607, -12597, -12594,
        -12585, -12556, -12359, -12346, -12320, -12300, -12120, -12099, -12089, -12074, 
        -12067, -12058, -12039, -11867, -11861, -11847, -11831, -11798, -11781, -11604,
        -11589, -11536, -11358, -11340, -11339, -11324, -11303, -11097, -11077, -11067, 
        -11055, -11052, -11045, -11041, -11038, -11024, -11020, -11019, -11018, -11014, 
        -10838, -10832, -10815, -10800, -10790, -10780, -10764, -10587, -10544, -10533,
        -10519, -10331, -10329, -10328, -10322, -10315, -10309, -10307, -10296, -10281,
        -10274, -10270, -10262, -10260, -10256, -10254
        };

        private static string[] pystr = new string[]
        {
        "a", "ai", "an", "ang", "ao", 

        "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng", "bi", "bian", 
        "biao", "bie", "bin", "bing", "bo", "bu",

        "ca", "cai", "can", "cang", "cao", "ce", "ceng", "cha", "chai", "chan", 
        "chang", "chao", "che", "chen", "cheng", "chi", "chong", "chou", "chu", 
        "chuai", "chuan", "chuang", "chui", "chun", "chuo", "ci", "cong", "cou", 
        "cu", "cuan", "cui", "cun", "cuo", 

        "da", "dai", "dan", "dang", "dao", "de", "deng", "di", "dian", "diao", 
        "die", "ding", "diu", "dong", "dou", "du", "duan", "dui", "dun", "duo", 

        "e", "en", "er", 

        "fa", "fan", "fang", "fei", "fen", "feng", "fo", "fou", "fu", 

        "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", "gong", 
        "gou", "gu", "gua", "guai", "guan", "guang", "gui", "gun", "guo",

        "ha", "hai", "han", "hang", "hao", "he", "hei", "hen", "heng", "hong", 
        "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun","huo",

        "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", 
        "jiu", "ju", "juan", "jue", "jun", 
        "ka", "kai", "kan","kang", "kao", "ke", "ken", "keng", "kong", "kou",
        "ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo", 

        "la", "lai", "lan","lang", "lao", "le", "lei","leng", "li", "lia", 
        "lian", "liang", "liao", "lie", "lin", "ling", "liu", "long", "lou", 
        "lu", "lv","luan", "lue", "lun", "luo",

        "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng", "mi", 
        "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu",

        "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", "ni",
        "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", "nu", 
        "nv", "nuan", "nue", "nuo", 

        "o", "ou", 

        "pa", "pai", "pan", "pang", "pao", "pei", "pen", "peng", "pi", "pian", 
        "piao", "pie", "pin", "ping", "po", "pu", 

        "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", "qing", "qiong", "qiu",
        "qu", "quan", "que", "qun",

        "ran", "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", "ru", 
        "ruan", "rui", "run", "ruo", 

        "sa", "sai", "san", "sang", "sao", "se", "sen", "seng", "sha", "shai", 
        "shan", "shang", "shao", "she", "shen", "sheng", "shi", "shou", "shu", 
        "shua", "shuai", "shuan", "shuang", "shui", "shun", "shuo", "si", 
        "song", "sou", "su", "suan", "sui", "sun", "suo", 

        "ta", "tai", "tan", "tang", "tao", "te", "teng", "ti", "tian", "tiao", 
        "tie", "ting", "tong", "tou", "tu", "tuan", "tui", "tun", "tuo",

        "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu", 

        "xi", "xia", "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", 
        "xiu", "xu", "xuan", "xue", "xun", 

        "ya", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo", "yong", 
        "you", "yu", "yuan", "yue", "yun", 

        "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", "zeng", "zha", 
        "zhai", "zhan", "zhang", "zhao", "zhe", "zhen", "zheng", "zhi", "zhong",
        "zhou", "zhu", "zhua", "zhuai", "zhuan", "zhuang", "zhui", "zhun", "zhuo",
        "zi", "zong", "zou", "zu", "zuan", "zui", "zun", "zuo"
        };
        #endregion

        /// <summary>
        /// ��һ������ת��Ϊƴ��
        /// ����������ַ�Ϊ�����ĺ��ֽ���ִ��ת����ֱ�ӷ���ԭ�ַ���
        /// </summary>
        /// <param name="chsstr">ָ������</param>
        /// <returns>ƴ����</returns>
        public static string ChsString2Spell(string chsstr)
        {
            string strRet = string.Empty;
            char[] ArrChar = chsstr.ToCharArray();
            foreach (char c in ArrChar)
            {
                strRet += SingleChs2Spell(c.ToString());
            }
            return strRet;
        }

        /// <summary>
        /// ��һ������ת��Ϊƴ��
        /// </summary>
        /// <param name="chsstr">ָ������</param>
        /// <returns>ƴ������ĸ</returns>
        public static string GetHeadOfChs(string chsstr)
        {
            string strRet = string.Empty;
            char[] ArrChar = chsstr.ToCharArray();
            foreach (char c in ArrChar)
            {
                strRet += GetHeadOfSingleChs(c.ToString());
            }
            return strRet;
        }

        /// <summary>
        /// ��������ת��Ϊƴ��
        /// </summary>
        /// <param name="SingleChs">��������</param>
        /// <returns>ƴ��</returns>
        public static string SingleChs2Spell(string SingleChs)
        {
            byte[] array;
            int iAsc;
            string strRtn = string.Empty;
            array = Encoding.Default.GetBytes(SingleChs);
            if (array.Length == 1) return SingleChs;
            try
            {
                iAsc = (short)(array[0]) * 256 + (short)(array[1]) - 65536;
            }
            catch
            {
                iAsc = 1;
            }
            if (iAsc > 0 && iAsc < 160)
                return SingleChs;
            for (int i = (pyvalue.Length - 1); i >= 0; i--)
            {
                if (pyvalue[i] <= iAsc)
                {
                    strRtn = pystr[i];
                    break;
                }
            }
            //������ĸתΪ��д
            if (strRtn.Length > 1)
            {
                strRtn = strRtn.Substring(0, 1).ToUpper() + strRtn.Substring(1);
            }
            return strRtn;
        }

        /// <summary>
        /// �õ���������ƴ��������ĸ
        /// </summary>
        /// <returns></returns>
        public static string GetHeadOfSingleChs(string SingleChs)
        {
            string s = SingleChs2Spell(SingleChs);
            if (s != "")
                return s.Substring(0, 1);
            else
                return "";
        }

    }
}