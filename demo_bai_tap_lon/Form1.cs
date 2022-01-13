using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Ports;
using System.Xml;
using ZedGraph;
using System.IO;


namespace demo_bai_tap_lon
{
    public partial class Form1 : Form
    {
        double time_read = 0;
        double data_read ;

        double gioi_han_tren = 19.00;
        double gioi_han_duoi = 4.00;
        double read_terminal;
        string read_data = "";
        string set_pid = "";
        string lenh_dk = "";
        double muc_set;

        double set_cao;
        public Form1()
        {
            InitializeComponent();
            ModifyProgressBarColor.SetState(verticalProgressBar1, 1);
        }

         string[]muc_nuoc_dat = { "muc_1(6 cm)" ,"muc_2(9 cm)","muc_3(12 cm)","muc_4(15 cm)","muc_full(18 cm)"};

        string[] list_baud = { "2400", "4800", "9600", "19200", "38400", "115200" };
        private void Form1_Load(object sender, EventArgs e)
        {
            string[] list_comname = SerialPort.GetPortNames();
            com_list.Items.AddRange(list_comname);
            baud_list.Items.AddRange(list_baud);
            com_set_muc.Items.AddRange(muc_nuoc_dat);


            GraphPane my_pane = zedGraphControl1.GraphPane;
            my_pane.Title.Text = "Biểu Đồ Mức Nước ";
            my_pane.YAxis.Title.Text = "Mức Nước (cm)";
            my_pane.XAxis.Title.Text = "thời gian (s)";

            RollingPointPairList list_1 = new RollingPointPairList(60000); 
            RollingPointPairList list_2 = new RollingPointPairList(60000);
   
            LineItem line_muc_nuoc = my_pane.AddCurve("MỨC NƯỚC", list_1, Color.Red, SymbolType.None); 
            LineItem line_muc_pid = my_pane.AddCurve("MỨC PID", list_2, Color.Blue, SymbolType.None);

            my_pane.XAxis.Scale.Min = 0;
            my_pane.XAxis.Scale.Max = 40;
            my_pane.XAxis.Scale.MinorStep = 1; 
            my_pane.XAxis.Scale.MajorStep = 5; 

            my_pane.YAxis.Scale.Min = 0;
            my_pane.YAxis.Scale.Max = 30;
            my_pane.YAxis.Scale.MinorStep = 1; 
            my_pane.YAxis.Scale.MajorStep = 3; 

            zedGraphControl1.AxisChange(); 

            label_ght.Text = gioi_han_tren.ToString();
            label_ghd.Text = gioi_han_duoi.ToString();
        }

        public void draw_zedgraph(double line1) 
        {
            LineItem line_muc_nuoc = zedGraphControl1.GraphPane.CurveList[0] as LineItem;
            if(line_muc_nuoc==null)
            {
                return;
            }
            IPointListEdit list1 = line_muc_nuoc.Points as IPointListEdit;
            if(list1==null)
            {
                return;
            }
            list1.Add(time_read,line1);

            if (set_pid != "")
            {
                LineItem line_muc_pid = zedGraphControl1.GraphPane.CurveList[1] as LineItem;
                IPointListEdit list_2 = line_muc_pid.Points as IPointListEdit;
                if (list_2 ==null) { return; }
                list_2.Add(time_read, double.Parse(set_pid));
            }

            Scale xscale = zedGraphControl1.GraphPane.XAxis.Scale;
            if(time_read>(xscale.Max-xscale.MajorStep))
            {
                xscale.Max = time_read + xscale.MajorStep;
                xscale.Min = xscale.Max - 40;
            }

            zedGraphControl1.AxisChange(); 
            zedGraphControl1.Invalidate();    
            zedGraphControl1.Refresh();

        }
        private void btn_connect_Click(object sender, EventArgs e)
        {
            if ((com_list.Text == "") || (baud_list.Text == ""))
            {
                MessageBox.Show("Vui Lòng Chọn Dữ Liệu Kết Nối", "Thông  Báo");
            }
            if (!serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.PortName = com_list.Text;
                    serialPort1.BaudRate = int.Parse(baud_list.Text);
                    serialPort1.Open();
                    lb_ket_noi.Text = "Đã Kết Nối";
                    lb_ket_noi.ForeColor = Color.Green;
                    progressBar1.Value = 100;
                    btn_connect.Enabled = false;
                    btn_disconect.Enabled = true;
                    lenh_dk = "Kết Nối";
                }
                catch (Exception)
                {
                    MessageBox.Show("Lỗi !! \n Vui Lòng Kiểm Tra Kết Nối", "ERROR!!!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btn_disconect_Click(object sender, EventArgs e)
        {
            if ((com_list.Text == "") || (baud_list.Text == ""))
            {
                MessageBox.Show("Vui Lòng Chọn Dữ Liệu Kết Nối", "Thông  Báo");
            }
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Close();
                    lb_ket_noi.Text = "Đã Ngắt Kết Nối";
                    lb_ket_noi.ForeColor = Color.Red;
                    progressBar1.Value = 0;
                    btn_connect.Enabled = true;
                    btn_disconect.Enabled = false;
                    lenh_dk = " Ngắt Kết Nối";
                }
                catch
                {
                }
            }
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            DialogResult tra_loi;
            tra_loi = MessageBox.Show("Ban Muốn Thoát Khỏi Chương Trình ??", "EXIT", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (tra_loi == DialogResult.OK)
            {
                Application.Exit();
            }
        }

        private void btn_on_bom_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (pic_bom_off.Visible==true) 
                {
                    serialPort1.Write("#BOMON"+"\n");
                    pic_bom_off.Visible = false;
                    pic_bom_on.Visible = true;
                    lenh_dk = "BƠM ON";
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_off_bom_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (pic_bom_on.Visible==true) 
                {
                    serialPort1.Write("#BOMOF"+"\n");
                    pic_bom_off.Visible = true;
                    pic_bom_on.Visible = false;
                    lenh_dk = "BƠM OFF";
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_on_van_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (pic_van_off.Visible==true) 
                {
                    serialPort1.Write("#VANON"+"\n");
                    pic_van_off.Visible = false;
                    pic_van_on.Visible = true;
                    lenh_dk = "VAN ON";
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
       
        private void btn_off_van_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (pic_van_on.Visible==true) 
                {
                    serialPort1.Write("#VANOF"+"\n");
                    pic_van_off.Visible = true;
                    pic_van_on.Visible = false;
                    lenh_dk = "VAN OFF";
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
       
        private void set_do_cao_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                if(set_do_cao.Text=="Set Mức")
                {
                    if (com_set_muc.Text!= "")
                    {
                        if (((com_set_muc.Text == "muc_1(6 cm)") && (gioi_han_tren >= 9.0) && (gioi_han_duoi < 5.0)) || ((com_set_muc.Text == "muc_2(9 cm)") && (gioi_han_tren >= 11.0) && (gioi_han_duoi < 7.0))
                            || ((com_set_muc.Text == "muc_3(12 cm)") && (gioi_han_tren >= 14.0) && (gioi_han_duoi < 10.0)) || ((com_set_muc.Text == "muc_4(15 cm)") && (gioi_han_tren >= 17.0) && (gioi_han_duoi < 13.0))
                            || ((com_set_muc.Text == "muc_full(18 cm)") && (gioi_han_tren >= 19.0) && (gioi_han_duoi < 16.0)))
                        {
                            set_do_cao.BackColor = Color.Orange;
                            set_do_cao.Text = "Dừng Set";
                            if (com_set_muc.Text == "muc_1(6 cm)")
                            {
                                serialPort1.Write("#MUCM1" + "\n");
                                muc_set = 6.0;
                                lenh_dk = "SET MỨC 1";
                            }
                            else if (com_set_muc.Text == "muc_2(9 cm)")
                            {
                                serialPort1.Write("#MUCM2" + "\n");
                                muc_set = 9.0;
                                lenh_dk = "SET MỨC 2";
                            }
                            else if (com_set_muc.Text == "muc_3(12 cm)")
                            {
                                serialPort1.Write("#MUCM3" + "\n");
                                muc_set = 12.0;
                                lenh_dk = "SET MỨC 3";
                            }
                            else if (com_set_muc.Text == "muc_4(15 cm)")
                            {
                                serialPort1.Write("#MUCM4" + "\n");
                                muc_set = 15.0;
                                lenh_dk = "SET MỨC 4";
                            }
                            else if (com_set_muc.Text == "muc_full(18 cm)")
                            {
                                serialPort1.Write("#MUCM5" + "\n");
                                muc_set = 18.0;
                                lenh_dk = "SET FULL";
                            }
                            if(muc_set>data_read)
                            {
                                set_cao = 1;
                            }else if(muc_set<data_read)
                            { 
                                set_cao = 2;
                            }
                            
                            com_set_muc.Enabled = false;
                            groupBox_set_gh.Enabled = false;
                            groupBox_manual.Enabled = false;
                            groupBox_set_pid.Enabled = false;

                            pic_bom_off.Visible = true;
                            pic_bom_on.Visible = false;
                            pic_van_off.Visible = true;
                            pic_van_on.Visible = false;
                        }
                        else
                        {
                            MessageBox.Show("Lỗi !!! \n Giá trị vượt quá phạm vi cho phép", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            com_set_muc.Text = "";
                        }
                    }
                    else
                    {
                        MessageBox.Show("Lỗi!! \n Chưa Chọn Mức Nước Cần Set", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else if(set_do_cao.Text=="Dừng Set")
                {
                    serialPort1.Write("#STO" + "\n");
                    set_do_cao.BackColor = Color.Aquamarine;
                    set_do_cao.Text = "Set Mức";
                    lenh_dk = "DỪNG SET MỨC";

                    muc_set = 0;
                    com_set_muc.Enabled = true;
                    groupBox_manual.Enabled = true;
                    groupBox_set_pid.Enabled = true;
                    groupBox_set_gh.Enabled = true;
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_set_muc_pid_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                if (data_muc_nuoc.Text != "")
                {
                    try
                    {
                        if (btn_set_muc_pid.Text == "Set Pid")
                        {
                            if ((double.Parse(data_muc_nuoc.Text) >=gioi_han_duoi+1.0) && (double.Parse(data_muc_nuoc.Text) <= gioi_han_tren-2.0)&&(data_muc_nuoc.Text.Length<=4))
                            {
                                serialPort1.Write("#PID" + data_muc_nuoc.Text + "\n");
                                btn_set_muc_pid.Text = "Dung Pid";
                                lenh_dk = "SET PID:" + data_muc_nuoc.Text + "cm";
                                btn_set_muc_pid.BackColor = Color.Orange;
                                set_pid = data_muc_nuoc.Text;

                                groupBox_set_muc_nuoc.Enabled = false;
                                data_muc_nuoc.Enabled = false;
                                groupBox_set_gh.Enabled = false;
                                groupBox_manual.Enabled = false;

                                pic_bom_off.Visible = true;
                                pic_bom_on.Visible = false;
                                pic_van_off.Visible = true;
                                pic_van_on.Visible = false;
                            }
                            else
                            {
                                MessageBox.Show("Lỗi !!! \n Giá trị vượt quá phạm vi cho phép", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                               data_muc_nuoc.Text = "";
                            }
                        }
                        else if(btn_set_muc_pid.Text == "Dung Pid")
                        {
                            serialPort1.Write("#NOP"+"\n");
                            btn_set_muc_pid.Text = "Set Pid";
                            lenh_dk = "DỪNG PID";
                            btn_set_muc_pid.BackColor = Color.Aquamarine;
                            set_pid=data_muc_nuoc.Text = "";

                            groupBox_set_muc_nuoc.Enabled = true;
                            data_muc_nuoc.Enabled =true;
                            groupBox_set_gh.Enabled = true;
                            groupBox_manual.Enabled = true;
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Lỗi !!! \n Vui Lòng Nhập Giá Trị Thực ", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        data_muc_nuoc.Text = "";
                    }
                }
                else 
                { 
                    MessageBox.Show("Lỗi !!! \n Chưa Nhập Giá Trị Set PID", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error); 
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_set_top_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                try
                {
                    if ((double.Parse(text_ght.Text) <= 19.5) &&( double.Parse(text_ght.Text)>= 7.00)&&(double.Parse(text_ght.Text)>=gioi_han_duoi+3)&&(text_ght.Text.Length<=4))
                    {
                        serialPort1.Write("#SE1" + text_ght.Text + "\n");
                        label_ght.Text = text_ght.Text ;
                        lenh_dk = "SET GHT=" + text_ght.Text+"cm";
                        gioi_han_tren = double.Parse(text_ght.Text);
                    }
                    else
                    {
                        MessageBox.Show("Lỗi !!! \n  Giá trị vượt quá phạm vi cho phép ", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        text_ght.Text = "";
                    }
                }
                catch
                {
                    MessageBox.Show("Lỗi !!! \n Vui Lòng Nhập Giá Trị Thực ", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    text_ght.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_set_bottom_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try
                {
                    if ((double.Parse(text_ghd.Text) <= 18.00) && (double.Parse(text_ghd.Text) >= 4.00)&&(double.Parse(text_ghd.Text) <=gioi_han_tren-3)&&(text_ghd.Text.Length<=4))
                    {
                        gioi_han_duoi = double.Parse(text_ghd.Text);
                        serialPort1.Write("#SE2" +text_ghd.Text+ "\n");
                        lenh_dk = "SET GHD=" + text_ghd.Text + "cm";
                        label_ghd.Text = text_ghd.Text ;
                    }
                    else
                    {
                        MessageBox.Show("Lỗi !!! \n  Giá trị vượt quá phạm vi cho phép ", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        text_ghd.Text = "";
                    }
                }
                catch
                {
                    MessageBox.Show("Lỗi !!! \n Vui Lòng Nhập Giá Trị Thực ", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    text_ghd.Text = "";
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_reset_Click(object sender, EventArgs e)
        {
            if(serialPort1.IsOpen)
            {
                serialPort1.Write("#RES"+"\n");
                text_ghd.Text=label_ghd.Text = 4.ToString() ;
                text_ght.Text=label_ght.Text = 19.ToString();
                lenh_dk = "RESET GIỚI HẠN";
                gioi_han_duoi = 4.0;
                gioi_han_tren = 19.0;

                lb_m1.Visible = true;
                lb_m4.Visible = true;
                lb_m3.Visible = true;
                lb_m2.Visible = true;
                lb_full.Visible = true;
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btn_thong_tin_Click(object sender, EventArgs e)
        {
            Form2 frm2 = new Form2();
            frm2.Show();
        }

        private void btn_save_excel_Click(object sender, EventArgs e)
        {
            DialogResult tra_loi;
            tra_loi = MessageBox.Show("Bạn Muốn Lưu Dữ Liệu Vào EXCEL ", "Save", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if(tra_loi==DialogResult.Yes)
            {
                save_excel();
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            read_data = serialPort1.ReadLine();
            double.TryParse(read_data, out read_terminal);
            data_read = Math.Round(read_terminal,2);
        }

        private void btn_reset_graph_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                if (read_data!="")
                {
                    time_read = 0;
                    clear_zedgraph();
                }
                else
                {
                }
            }
            else
            {
                MessageBox.Show("Chưa Kết Nối Cổng COM", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void save_setting()
        {
            Properties.Settings.Default.Comname = com_list.Text;
            Properties.Settings.Default.Save();
        }

        private void save_excel()
        {

            Microsoft.Office.Interop.Excel.Application so_lieu = new Microsoft.Office.Interop.Excel.Application();
            so_lieu.Visible = true;
            Microsoft.Office.Interop.Excel.Workbook wb = so_lieu.Workbooks.Add(Microsoft.Office.Interop.Excel.XlSheetType.xlWorksheet);

            Microsoft.Office.Interop.Excel.Worksheet ws = (Microsoft.Office.Interop.Excel.Worksheet)so_lieu.ActiveSheet;

           Microsoft.Office.Interop.Excel.Range rg = (Microsoft.Office.Interop.Excel.Range)ws.get_Range("A1","C1");

            ws.Cells[1,1] =  "Thời Gian (s)        ";
            ws.Cells[1, 2] = "Mức Nước (cm)   ";
            ws.Cells[1, 3] = "Lệnh Điều Khiển        ";
            rg.Columns.AutoFit();
            int i = 2;
            int j = 2;
            foreach(ListViewItem comp in listView1.Items)
            {
                ws.Cells[i, j] = comp.Text.ToString();
                foreach(ListViewItem.ListViewSubItem drv in comp.SubItems)
                {
                    ws.Cells[i, j] = drv.Text.ToString();
                    j++;
                }
                j = 1;
                i++;
            }
        }

        private void data_list_view()
        {
            if(serialPort1.IsOpen)
            {
                ListViewItem item = new ListViewItem(lb_time.Text);
                item.SubItems.Add(data_read.ToString());
                item.SubItems.Add(lenh_dk);
                listView1.Items.Add(item);
                listView1.Items[listView1.Items.Count - 1].EnsureVisible();
            }
        }

        private void clear_zedgraph()
        {
            zedGraphControl1.GraphPane.CurveList.Clear();
            zedGraphControl1.GraphPane.GraphObjList.Clear();

            zedGraphControl1.AxisChange();
            zedGraphControl1.Invalidate();

            GraphPane my_pane = zedGraphControl1.GraphPane;
            my_pane.Title.Text = "ĐỒ THỊ MỨC NƯỚC ";
            my_pane.XAxis.Title.Text = "Thời Gian (s)    ";
            my_pane.YAxis.Title.Text = "Mức Nước (cm)";

            RollingPointPairList list_1 = new RollingPointPairList(60000);
            RollingPointPairList list_2 = new RollingPointPairList(60000);

            LineItem line_muc_nuoc = my_pane.AddCurve("MỨC NƯỚC", list_1, Color.Red, SymbolType.None);
            LineItem line_muc_PID = my_pane.AddCurve("MỨC PID", list_2, Color.Blue, SymbolType.None);

            my_pane.XAxis.Scale.Min = 0;
            my_pane.XAxis.Scale.Max = 40;
            my_pane.XAxis.Scale.MinorStep = 1;
            my_pane.XAxis.Scale.MajorStep = 5;

            my_pane.YAxis.Scale.Min = 0;
            my_pane.YAxis.Scale.Max = 30;
            my_pane.YAxis.Scale.MinorStep = 1;
            my_pane.YAxis.Scale.MajorStep = 3;
        }

            private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Start();
            lb_date.Text = DateTime.Now.ToLongDateString();
            lb_time.Text = DateTime.Now.ToLongTimeString();
            if (serialPort1.IsOpen)
            {
                if (data_read != 0)
                {
                    draw_zedgraph(data_read);
                    data_list_view();
                    label_hien_thi_muc_nuoc.Text = data_read.ToString();
                    label_the_tich.Text = (data_read * 180 + 1220).ToString();
                }
            }
            else
            {
            }
            if(lenh_dk!="")
            {
                lenh_dk = "";
            }   
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            timer2.Start();
            if (data_read != 0)
            {
                time_read++;
            }
        }
        private void timer3_Tick(object sender, EventArgs e)
        {
            timer3.Start();
            try
            {
                double test = Math.Round(double.Parse(read_data),1);
                verticalProgressBar1.Value = (int)test * 4;
                if((data_read>=6.0)&&(data_read<9.0))
                {
                    lb_m1.ForeColor = Color.Orange;
                }
                else { lb_m1.ForeColor = Color.Black; }
                if ((data_read >= 9.0) && (data_read < 12.0))
                {
                    lb_m2.ForeColor = Color.Orange;
                }
                else { lb_m2.ForeColor = Color.Black; }
                if ((data_read >= 12.0) && (data_read < 15.0))
                {
                    lb_m3.ForeColor = Color.Orange;
                }
                else { lb_m3.ForeColor = Color.Black; }
                if ((data_read >= 15.0) && (data_read < 18.0))
                {
                    lb_m4.ForeColor = Color.Orange;
                }
                else { lb_m4.ForeColor = Color.Black; }
                if ((data_read >= 18.0) && (data_read < 19.0))
                {
                    lb_full.ForeColor = Color.Orange;
                }
                else { lb_full.ForeColor = Color.Black; }

            }
            catch { }
            if(serialPort1.IsOpen)
            {
                if(data_read!=0)
                { 
                    if ((data_read <= gioi_han_duoi+1.0))
                    {
                        ModifyProgressBarColor.SetState(verticalProgressBar1, 3);
                        pic_van_on.Visible = false;
                        pic_van_off.Visible = true;
                        groupBox_control_van.Enabled = false;
                    }
                    else if ((data_read >= gioi_han_tren - 1.0)&&(data_read<gioi_han_tren))
                    {
                        ModifyProgressBarColor.SetState(verticalProgressBar1, 2);
                        groupBox_control_bom.Enabled = true;
                    }
                    else if(data_read>=gioi_han_tren)
                    {
                        ModifyProgressBarColor.SetState(verticalProgressBar1, 2);
                        pic_bom_on.Visible = false;
                        pic_bom_off.Visible = true;
                        groupBox_control_bom.Enabled = false;
                    }    
                    else
                    {
                        ModifyProgressBarColor.SetState(verticalProgressBar1, 1);
                        groupBox_control_van.Enabled = true;
                        groupBox_control_bom.Enabled = true;
                    }
                }
            }
            if(serialPort1.IsOpen)
            {
                if(data_read!=0)
                {
                    if (((data_read >= gioi_han_duoi+1.3) && (data_read <= gioi_han_tren - 1.3)))
                    {
                        timer4.Enabled = true;
                    }
                    if((data_read<=gioi_han_tren-0.3)&&(data_read>=gioi_han_duoi+0.3))
                    {
                        timer6.Enabled = true;
                    }
                }
            } 
            if(serialPort1.IsOpen)
            {
                if(data_read!=0)
                {
                    if(((muc_set<= data_read)&&(muc_set!=0)&&(set_cao==1))||
                        ((muc_set-0.1>= data_read)&& (muc_set != 0) && (set_cao == 2)))
                    {
                        serialPort1.Write("#STO" + "\n");
                        set_do_cao.BackColor = Color.Aquamarine;
                        set_do_cao.Text = "Set Mức";
                        lenh_dk = "ĐẠT MỨC SET";

                        muc_set = 0;
                        set_cao = 1;
                        com_set_muc.Enabled = true;
                        groupBox_manual.Enabled = true;
                        groupBox_set_pid.Enabled = true;
                        groupBox_set_gh.Enabled = true;
                    }
                }
            }
        }
        private void timer4_Tick(object sender, EventArgs e)
        {
            timer4.Start();
            if (data_read != 0)
            {
                if ((data_read <= gioi_han_duoi+1.0)&&(data_read>gioi_han_duoi))
                {
                    timer4.Enabled=false;
                    MessageBox.Show("Nước Gần  Cạn !!! ", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if ((data_read >= gioi_han_tren - 1.0) && (data_read < gioi_han_tren))
                {
                    timer4.Enabled = false;
                    MessageBox.Show("Nước Gần Tràn !!! ", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void timer5_Tick(object sender, EventArgs e)
        {
            timer5.Start();
            if (serialPort1.IsOpen)
            {
                if ((text_ghd.Text == label_ghd.Text))
                {
                    int location_ghd = (int)double.Parse(text_ghd.Text);
                    lb_ghd.Location = new Point(1, 316 - location_ghd * 12);
                    if (location_ghd <= 4)
                    {
                        lb_m1.Visible = true;
                    }
                    else if (location_ghd <= 5)
                    {
                        lb_m1.Visible = false;
                        lb_m2.Visible = true;
                    }
                    else if (location_ghd <= 6)
                    {
                        lb_m1.Visible = false;
                        lb_m2.Visible = false;
                        lb_m3.Visible = true;
                    }
                    else if (location_ghd <= 9)
                    {
                        lb_m1.Visible = false;
                        lb_m2.Visible = false;
                        lb_m3.Visible = false;
                        lb_m4.Visible = true;
                    }
                    else if (location_ghd <= 12)
                    {
                        lb_m1.Visible = false;
                        lb_m4.Visible = false;
                        lb_m3.Visible = false;
                        lb_m2.Visible = false;
                        lb_full.Visible = true;
                    }
                    else if (location_ghd <= 15)
                    {
                        lb_m1.Visible = false;
                        lb_m4.Visible = false;
                        lb_m3.Visible = false;
                        lb_m2.Visible = false;
                        lb_full.Visible = false;
                    }
                }
                if (text_ght.Text == label_ght.Text)
                {
                    int location = (int)double.Parse(text_ght.Text);
                    lb_ght.Location = new Point(7, 316 - location * 12);
                    if (location >= 19)
                    {
                        lb_full.Visible = true;
                    }
                    else if (location >= 17)
                    {
                        lb_full.Visible = false;
                        lb_m4.Visible = true;
                    }
                    else if (location >= 14)
                    {
                        lb_full.Visible = false;
                        lb_m4.Visible = false;
                        lb_m3.Visible = true;
                    }
                    else if (location >= 11)
                    {
                        lb_full.Visible = false;
                        lb_m4.Visible = false;
                        lb_m2.Visible = true;
                        lb_m3.Visible = false;
                    }
                    else if (location >= 8)
                    {
                        lb_full.Visible = false;
                        lb_m4.Visible = false;
                        lb_m3.Visible = false;
                        lb_m2.Visible = false;
                        lb_m1.Visible = true;
                    }
                    else if (location >= 6)
                    {
                        lb_full.Visible = false;
                        lb_m4.Visible = false;
                        lb_m3.Visible = false;
                        lb_m2.Visible = false;
                        lb_m1.Visible = false;
                    }
                }
            }
        }
        private void timer6_Tick(object sender, EventArgs e)
        {
            timer6.Start();
            if(data_read!=0)
            {
                if(data_read>=gioi_han_tren)
                {
                    timer6.Enabled = false;
                    MessageBox.Show("Nước Tràn !!! ", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                if(data_read<=gioi_han_duoi)
                {
                    timer6.Enabled = false;
                    MessageBox.Show("Nước Cạn !!! ", "WARNING", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }    
            }
        }
    }
}
