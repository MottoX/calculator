using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace CALCULATOR
{
    public partial class frm_cal : Form
    {
        public frm_cal()
        {
            InitializeComponent();
            optr.Push('=');
            //为避免重复代码影响版面，将10个button引用赋值为操作区的0到9，并统一设置click方法。
            btn_num[0] = btn0;
            btn_num[1] = btn1;
            btn_num[2] = btn2;
            btn_num[3] = btn3;
            btn_num[4] = btn4;
            btn_num[5] = btn5;
            btn_num[6] = btn6;
            btn_num[7] = btn7;
            btn_num[8] = btn8;
            btn_num[9] = btn9;
            for (int i = 0; i <= 9; i++)
                btn_num[i].Click += btn_num_Click;
            result.Text = "0";
            screen.Text = "";
        }
        #region 成员变量声明
        private Stack<double> num = new Stack<double>();//表达式计算中需要的数字栈
        private Stack<char> optr = new Stack<char>();//表达式计算中需要的操作符栈，为方便编程，栈底元素为‘=’
        private Button[] btn_num = new Button[10];//统一设置0~9用的button引用
        private readonly double PI = Math.PI;
        private readonly double E = Math.E;
        private int lb_num = 0;//为表达式中括号匹配标记左括号数量
        private int rb_num = 0;//右括号数量
        private bool op_pressed = false;//表示是否当前按下了操作符，包括功能区的幂与根式
        private bool func_pressed = false;//表示是否按下了功能键，不包括幂与根式
        private bool num_editing = false; //表示当前是否正在编辑操作数，初始为false
        private bool finish_calulation = true;//表示是否已经完成了表达式运算，初始为true
        #endregion
        #region 表达式运算与修改核心代码
        private char get_priority(char a, char b)
        {
            if (a == '+' || a == '-')
            {
                if (b == '*' || b == '/' ||b=='%'|| b == 'r' || b == '^')
                    return '<';
                else if (a == b) return '=';
                else return '>';
            }
            else if (a == '*' || a == '/'||a=='%')
            {
                if (b == 'r' || b == '^')
                    return '<';
                else if (a == b) return '=';
                else return '>';
            }
            else if (a == 'r' || a == '^') return a == b ? '=' : '>';
            return 'E';
        }
        private void optr_cal(char ch)
        {
            double p = num.Pop(), q = num.Pop();
            switch (ch)
            {   
                case '+': num.Push(p + q); break;
                case '-': num.Push(q - p); break;
                case '*': num.Push(q * p); break;
                case '/': num.Push(q / p); break;
                case '%': num.Push(q % p); break;
                case '^': num.Push(Math.Pow(q, p)); break;
                case 'r': num.Push(Math.Pow(q, 1 / p)); break;//操作符r表示按键中的x^(1/y) 即yroot根式运算符
            }
            result.Text = Convert.ToString(num.Peek());//将新计算出的操作数打印到数字屏
        }
        private void dynamic_cal(char sign)
        {
            if (op_pressed == true)
            {
                char last_op = screen.Text[screen.Text.Length - 2];
                bool add_bracket = get_priority(last_op,sign)=='<';//判断是否需要加括号
                screen.Text = screen.Text.Remove(screen.Text.Length - 3); //因为之前按过运算符了，总之先删去
                if (add_bracket == true)   //如果需要加括号，则寻找加括号的正确位置并加上
                {
                    int pos = screen.Text.Length - 1;
                    while (pos > 0) 
                    { 
                        pos = get_pos(pos); 
                        if (pos > 0 && screen.Text[pos - 1] == '(') break; 
                        pos--; 
                    }
                    pos = pos >= 0 ? pos : 0;
                    double tmp=0;
                    //MessageBox.Show(screen.Text.Substring(pos, screen.Text.Length - pos) + "nani"+Double.TryParse(screen.Text.Substring(pos,screen.Text.Length-pos),out tmp).ToString());
                    if (Double.TryParse(screen.Text.Substring(pos,screen.Text.Length-pos),out tmp)==false )
                        screen.Text = screen.Text.Remove(pos) + '(' + screen.Text.Substring(pos, screen.Text.Length - pos) + ')';
                }
                optr.Pop();
            }
            else if (num_editing == true || finish_calulation==true || (screen.Text != "" && screen.Text[screen.Text.Length - 1] == '('))
            {
                num.Push(Convert.ToDouble(result.Text));
                if (func_pressed == false)
                    screen.Text += result.Text; 
            }
            screen.Text += " " + sign + " ";
            op_pressed = true;
            func_pressed = false;
            num_editing = false;
            char ch = optr.Peek();
            while (ch != '=' && ch != '(' && get_priority(optr.Peek(), sign) != '<')
            {
                optr_cal(ch);
                optr.Pop();
                ch = optr.Peek();
            }
            optr.Push(sign);
        }
        private void plus_Click(object sender, EventArgs e)
        {
            dynamic_cal('+');
        }
        private void minus_Click(object sender, EventArgs e)
        {
            dynamic_cal('-');
        }
        private void multiple_Click(object sender, EventArgs e)
        {
            dynamic_cal('*');
        }
        private void divide_Click(object sender, EventArgs e)
        {                                      
            dynamic_cal('/');
        }
        private void btn_mod_Click(object sender, EventArgs e)
        {                                          
            dynamic_cal('%');
        }
        private void btneql_Click(object sender, EventArgs e)
        {
            //如果当前正在编辑操作数或者表达式最后一位是左括号，也即少操作数，将当前操作数压入数字栈中
            if (num_editing==true || op_pressed==true || (screen.Text!=""&&screen.Text[screen.Text.Length-1]=='('))
                num.Push(Convert.ToDouble(result.Text));
            //取操作符栈顶元素
            char ch = optr.Peek();
            //处理表达式，直到操作符栈到底
            while (ch != '=')
            {
                if (ch!='(')//以防万一表达式中括号不匹配，例如按下1+（2=
                    optr_cal(ch);
                optr.Pop();
                ch = optr.Peek();
            }
            result.Text = Convert.ToString(num.Peek());
            screen.Text = "";
            //恢复两个表达式栈
            num.Clear();
            optr.Clear();
            optr.Push('=');
            //初始化左右括号数量及其它boolean变量
            lb_num = rb_num = 0;
            op_pressed = false;
            func_pressed = false;
            finish_calulation = true;
        }
        private void btn_lb_Click(object sender, EventArgs e)
        {
            if (op_pressed==false&&(screen.Text.Length>0&&screen.Text[screen.Text.Length-1]!='('))//如果不在按下操作符情况下（即其实这时不应按下括号），再按括号，即例如1+sin(5)，此时再按(，则应消去sin(5)即末项
            {
                int pos = get_pos();
                screen.Text = screen.Text.Remove(pos) + '(';
                func_pressed = false;
                result.Text = "0";
            }
            else 
            {
                screen.Text += '(';
                result.Text = "0";
            }
            optr.Push('(');
            lb_num++;
            op_pressed = false;
            finish_calulation = false;
            num_editing = false;
        }
        private void btn_rb_Click(object sender, EventArgs e)
        {
            if (rb_num >= lb_num) return;//能按下右括号前提是数量比左括号少
            Button btn = (Button)sender;
            if (num_editing == true ||op_pressed==true||(screen.Text != "" && screen.Text[screen.Text.Length - 1] == '('))//op_pressed防止(1+再按括号导致崩溃
                num.Push(Convert.ToDouble(result.Text));
            if (func_pressed == true || screen.Text[screen.Text.Length - 1] == ')')
            {//如果当前按下了功能键或者右括号，则直接在表达式加上)，例如(1+sin(5)变为(1+sin(5))以及1+((0)变为1+((0))
                screen.Text += btn.Text;
            }
            else
            {                      //否则，需要将数字屏上的内容打印上去，再加括号，如(1+变为(1+5),如果你刚才按的操作数是5的话。
                screen.Text += result.Text + btn.Text;
            }
            //如果当前正在编辑操作数或者表达式最后一位是左括号，也即少操作数，将当前操作数压入数字栈中
            char ch = optr.Peek();                  //开始计算加上右括号后，运算出的结果
            while (ch != '(' && ch != '=')
            {
                optr_cal(ch);
                optr.Pop();
                ch = optr.Peek();
            }
            optr.Pop();
            rb_num++;
            num_editing = false;
            func_pressed = false;
            op_pressed = false;
            finish_calulation = false;
        }
        private int get_pos(int start=0)//此方法非常重要，返回表达式中最后一项的起始位置。
        {
            int pos;
            if (start == 0)
                pos = screen.Text.Length - 1;
            else pos = start;
            int bracket_num = 0;
            while (pos > 0)//定位的方法很简单，前一个字符为空格，这是新一项的必要条件，并且当前括号匹配或者当前为括号，并且就差这个括号
            {
                if (screen.Text[pos] == ')')
                    bracket_num++;
                else if (screen.Text[pos] == '(')
                    bracket_num--;
                if ((screen.Text[pos - 1] == ' '||screen.Text[pos-1]=='(') && bracket_num == 0 )
                        break;
                if (bracket_num < 0) return pos + 1;
                pos--;
            }
            return pos;
        }
        #endregion
        #region 操作区代码
        #region 数字区统一方法 
        private void btn_num_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            //如果按下了exp，由于exp按下后，初始为X.e+0格式，故需要将0取代
            if (result.Text.Length >= 3 && result.Text[result.Text.Length - 3] == 'e' && result.Text[result.Text.Length - 1] == '0')
            {
                result.Text = result.Text.Remove(result.Text.Length - 1);
                result.Text += btn.Text;
            }
            else if (op_pressed == true)//如果当前按下了操作符+-*/包括括号，则需要将数字屏中的原来数字清空，更新为新的操作数
                result.Text = btn.Text;
            else if (finish_calulation == true)//如果完成了某次运算，当然也是需要将数字屏清空，更新为新的操作数
            {
                screen.Text = "";
                result.Text = btn.Text;
            }
            else if (func_pressed == true)//如果按下的是功能键，则将最后一项取代
            {
                int pos = get_pos();
                screen.Text = screen.Text.Remove(pos);
                result.Text = btn.Text;
            }
            //其余的便是键入操作数了，只需要将按下的数字加到原有数字尾部即可，如果原本是0，则取代
            else if (result.Text == "0")
                result.Text = btn.Text;
            else result.Text += btn.Text;
            //统一赋值变量
            finish_calulation = false;
            func_pressed = false;
            op_pressed = false;
            num_editing = true;
        }
        #endregion
        private void btn_pt_Click(object sender, EventArgs e)
        {   //如果刚才按下的是功能键，则将最后一项删除，并加上0.以保证表达式合法并且计算器不会因为用户这样的操作而崩溃
            if (func_pressed == true)
            {
                int pos = get_pos();
                screen.Text = screen.Text.Remove(pos);
                result.Text = "0.";
                func_pressed = false;
                return;
            }
            else if (result.Text.IndexOf('.') != -1) return;//如果当前操作数已经有小数点了，无视
            Button btn = (Button)sender;
            //如果当前按下了操作符，即需要键入新的操作数，则按下小数点默认为0.XXX
            if (op_pressed == true)
            {
                result.Text = "0" + btn.Text;
                op_pressed = false;
            }
            else//其余则直接加上小数点
                result.Text += btn.Text;
            finish_calulation = false;
            num_editing = true;
        }
        private void back_Click(object sender, EventArgs e)
        {
            if (op_pressed == true || func_pressed == true || finish_calulation == true)//如果按下了运算符或者功能键或者运算完毕，无视
                return;
            if (result.Text.Length == 1)
                result.Text = "0";
            else
                result.Text = result.Text.Remove(result.Text.Length - 1);
        }
        private void clear_Click(object sender, EventArgs e)
        {   //click函数将所有需要的变量初始化
            screen.Text = "";
            result.Text = "0";
            num.Clear();
            optr.Clear();
            optr.Push('=');
            lb_num = rb_num = 0;
            op_pressed = false;
            func_pressed = false;
            finish_calulation = true;
            num_editing = false;
        }
        #endregion
        #region 功能区代码
        //此方法用来修正表达式
        private void expr_mod(object sender, double ans, String txt = "empty")
        {
            Button btn = (Button)sender;
            String func = txt == "empty" ? btn.Text : txt;
            char ch;
            if (screen.Text == "" || (ch = screen.Text[screen.Text.Length - 1]) == ' ' || ch == '(')
                screen.Text += func + "(" + result.Text + ")";//如果表达式结尾是空格或者左括号，也即即将输入新的一项，则将功能函数套入表达式
            else                                            //其余情况则将功能函数xxx()套上表达式最后一项
            {
                int pos = get_pos();
                String part = screen.Text.Substring(pos, screen.Text.Length - pos );
                if (ch == ')' && func_pressed == false)// 如果最后一项并不是功能函数，并且是以括号形式出现的，则不需要功能函数的括号
                    part = func + part;
                else part = func + "(" + part + ")";//否则，在最后一项外面套上括号再加上功能函数名
                screen.Text = screen.Text.Remove(pos)+part;//去掉最后一项,补上套上功能函数的最后一项
            }
            //if (num_editing == false&&num.Count>0) num.Pop();
            result.Text = Convert.ToString(ans);//将运算后的值显示到数字屏上
            func_pressed = true;
            op_pressed = false;
            num_editing = true;
        }
        private void btn_sin_Click(object sender, EventArgs e)
        {
            double now = Convert.ToDouble(result.Text);
            double ans;
            if (radioButton1.Checked == true)
                ans = Math.Sin(now * PI / 180);
            else ans = Math.Sin(now);
            if (Math.Abs(ans) < 1e-6) ans = 0;
            expr_mod(sender, ans);//在运算出ans后调用表达式修正方法
        }
        private void btn_cos_Click(object sender, EventArgs e)
        {
            double now = Convert.ToDouble(result.Text);
            double ans;
            if (radioButton1.Checked == true)
                ans = Math.Cos(now * PI / 180);
            else ans = Math.Cos(now);
            if (Math.Abs(ans) < 1e-6) ans = 0;
            expr_mod(sender, ans);//在运算出ans后调用表达式修正方法
        }
        private void btn_tan_Click(object sender, EventArgs e)
        {
            double now = Convert.ToDouble(result.Text);
            double ans;
            if (radioButton1.Checked == true)
                ans = Math.Tan(now * PI / 180);
            else ans = Math.Tan(now);
            if (Math.Abs(ans) < 1e-6) ans = 0;
            expr_mod(sender, ans);//在运算出ans后调用表达式修正方法
        }
        private void btn_asin_Click(object sender, EventArgs e)
        {
            double now = Convert.ToDouble(result.Text);
            double ans = Math.Asin(now);
            if (radioButton1.Checked == true)
                ans = ans * 180 / PI;
            expr_mod(sender, ans);//在运算出ans后调用表达式修正方法
        }
        private void btn_acos_Click(object sender, EventArgs e)
        {
            double now = Convert.ToDouble(result.Text);
            double ans = Math.Acos(now);
            if (radioButton1.Checked == true)
                ans = ans * 180 / PI;
            expr_mod(sender, ans);//在运算出ans后调用表达式修正方法
        }
        private void btn_atan_Click(object sender, EventArgs e)
        {
            double now = Convert.ToDouble(result.Text);
            double ans = Math.Atan(now);
            if (radioButton1.Checked == true)
                ans = ans * 180 / PI;
            expr_mod(sender, ans);//在运算出ans后调用表达式修正方法
        }
        private void btn_square_Click(object sender, EventArgs e)
        {
            double ans = Convert.ToDouble(result.Text);
            ans *= ans;
            expr_mod(sender, ans, "sqr");//在运算出ans后调用表达式修正方法，并且第三参数设为sqr
        }
        private void btn_ln_Click(object sender, EventArgs e)
        {
            double ans = Convert.ToDouble(result.Text);
            ans = Math.Log(ans);
            expr_mod(sender, ans);//在运算出ans后调用表达式修正方法
        }
        private void btn_log_Click(object sender, EventArgs e)
        {
            double ans = Convert.ToDouble(result.Text);
            ans = Math.Log10(ans);
            expr_mod(sender, ans);//在运算出ans后调用表达式修正方法
        }
        private void btn_fact_Click(object sender, EventArgs e)
        {
            double num = (int)Convert.ToDouble(result.Text);
            double ans = 1;
            for (int i = 1; i <= num; i++) ans = ans * i;
            expr_mod(sender, ans, "fact");//在运算出ans后调用表达式修正方法，并且第三参数设为fact
        }
        private void btn_sqrt_Click(object sender, EventArgs e)
        {
            double ans = Convert.ToDouble(result.Text);
            ans = Math.Sqrt(ans);
            expr_mod(sender, ans, "sqrt");//在运算出ans后调用表达式修正方法，并且第三参数设为sqrt
        }
        private void btn_pow10_Click(object sender, EventArgs e)
        {
            double ans = Math.Pow(10, Convert.ToDouble(result.Text));
            expr_mod(sender, ans, "powten");//在运算出ans后调用表达式修正方法，并且第三参数设为powten
        }
        private void btn_powe_Click(object sender, EventArgs e)
        {
            double ans = Math.Pow(E, Convert.ToDouble(result.Text));
            expr_mod(sender, ans, "powe");////在运算出ans后调用表达式修正方法，并且第三参数设为powe
        }
        private void btn_pow_Click(object sender, EventArgs e)
        {                                         
            dynamic_cal('^');
        }
        private void btn_root_Click(object sender, EventArgs e)
        {
            dynamic_cal('r');
        }
        private void btn_exp_Click(object sender, EventArgs e)
        {
            if (result.Text.IndexOf('e') != -1) return;
            if (result.Text.IndexOf('.') == -1) result.Text += '.';
            result.Text += "e+0";

        }
        private void btn_PI_Click_1(object sender, EventArgs e)
        {
            result.Text = Convert.ToString(PI);
            if (func_pressed == true)
                screen.Text = screen.Text.Remove(get_pos());
            op_pressed = false;
            func_pressed = false;
        }
        private void btn_e_Click_1(object sender, EventArgs e)
        {
            result.Text = Convert.ToString(E);
            if (func_pressed == true)
                screen.Text = screen.Text.Remove(get_pos());
            op_pressed = false;
            func_pressed = false;
        }
        private void btn_sign_Click(object sender, EventArgs e)
        {
            if (result.Text == "0") return; //如果数字屏为0，不处理
            int pos = result.Text.IndexOf('e');
            if (pos != -1)                                //如果之前按的是Exp按键
            {
                char ch = result.Text[pos + 1] == '+' ? '-' : '+';//判断原来是+还是-
                result.Text = result.Text.Remove(pos + 1) + ch + result.Text.Substring(pos + 2);
            }
            else if (op_pressed == true || func_pressed == true ||finish_calulation==true||
                (screen.Text.Length!=0 && (screen.Text[screen.Text.Length - 1] == '(' || screen.Text[screen.Text.Length - 1] == ')')))
                expr_mod(sender, -Convert.ToDouble(result.Text), "negate");
            else if (result.Text[0] != '-')//否则一般情况下就直接在数字前替换符号
                result.Text = result.Text.Insert(0, "-");
            else result.Text = result.Text.Remove(0, 1);
        }
        #endregion
        #region 菜单区代码
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("作者：Robin Wang(github.com/MottoX)",
                            "关于\"calculator\"");
        }
        
        private void menu_size_change_Click(object sender, EventArgs e)
        {
            if (menu_size_change.Text == "切换竖版")
            {
                this.Size = new Size(grp_fn.Width + 30, grp_fn.Location.Y + grp_fn.Height * 2 + 55);
                grp_op.Location = new Point(grp_fn.Location.X, grp_fn.Location.Y + grp_fn.Height + 10);
                screen.Width = grp_fn.Location.X + grp_fn.Width - 10;
                result.Width = screen.Width;
                Font new_font=new Font("楷体",10);
                screen.Font = new_font;
                menu_size_change.Text = "切换横版";
            }
            else
            {
                this.Size = new Size(grp_fn.Width*2+40,grp_fn.Location.Y+grp_fn.Height+45);
                grp_op.Location = new Point(grp_fn.Location.X+grp_fn.Width+10,grp_fn.Location.Y);
                screen.Width = grp_op.Location.X + grp_op.Width -10;
                result.Width = screen.Width;
                Font new_font=new Font("楷体",20);
                screen.Font = new_font;
                menu_size_change.Text = "切换竖版";
            }
        }
        #endregion
    }
}