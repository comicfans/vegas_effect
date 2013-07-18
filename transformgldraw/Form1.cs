using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;


namespace ConsoleApplication1
{
    public partial class Form1 : Form
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
 
            
        MyGLControl control = new MyGLControl();

        public Form1()
        {
            InitializeComponent();


            control.Dock = DockStyle.Fill;

            Controls.Add(control);

        }


        protected override void OnLoad(EventArgs e) {

            control.Init();

        }


        public class MyGLControl : OpenTK.GLControl { 
            int m_ImageWidth = 640;
        int m_ImageHeight = 512;
               
        
        bool m_Closed = false;
        bool m_Initialized = false;
        double m_TotalDrawTime;
        
        public double TotalDrawTime
        {
            get
            {
                return m_TotalDrawTime;
            }
        }

        
        protected override void OnResize(EventArgs e)
        {
            if (m_Initialized == false)
                return;

            MakeCurrent();
            GL.Viewport(0, 0, this.ClientSize.Width, this.ClientSize.Height);


        }
        

        protected override void OnPaint(PaintEventArgs e)
        {
            if (m_Initialized == false)
                return;
            
            //计算OpenGL绘制一帧的时间
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            MakeCurrent();
            Render();
            SwapBuffers();            

            sw.Stop();
            m_TotalDrawTime = GetAcurateTime(sw);
            //Console.WriteLine("Elapsed Time (ms): {0}", sw.ElapsedMilliseconds);            
        }

        public bool Initialized
        {
            get
            {
                return m_Initialized;
            }
        }


        //此函数只应该在启动的时候被调用一次，不允许多次调用
        public void Init()
        {
            if (IsHandleCreated == false)
            {
                MessageBox.Show("Handle does not exist.");
                return;
            }

            if (m_Initialized)
                return;

            MakeCurrent();
            //在这里判断是否显卡或者显卡驱动支持OpenGL 2.0以及所用到的OpenGL 扩展
            bool isSupportedGPU = true;
            //OpenTK对版本的判断有bug，暂时先不检查版本号
            //if (!GL.SupportsExtension("version20"))
            //    isSupportedGPU = false;    
            if (isSupportedGPU == false)
            {
                System.Windows.Forms.MessageBox.Show(
                     "Your video card does not support OpenGL 2.0. Please update your video card drivers or purchase a new video card.",
                     "OpenGL 2.0 not supported",
                     System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Exclamation);
                return;
            }

            initOpenGL(); 
                        
            m_Initialized = true;
        }        

        virtual protected void initOpenGL()
        {
            //先初始化一些整个程序中都不改变的OpenGL状态
            //GL.Enable((EnableCap)TextureTarget.TextureRectangleArb);
            GL.Disable(EnableCap.Texture2D);
            GL.Disable(EnableCap.DepthTest);
            GL.Disable(EnableCap.Lighting);            
            //GL.Arb.ClampColor(ArbColorBufferFloat.ClampFragmentColorArb, (ArbColorBufferFloat)ClampColorMode.False);
            GL.ClearColor(0.0f, 0.0f, 1.0f, 1.0f);

            GL.Ortho(-512, 512, -384, 384, -1, 1);
            GL.Viewport(0, 0, this.ClientSize.Width, this.ClientSize.Height);
        }         

        virtual protected void closeOpenGL()
        {
            //调用OpenGL函数之前，必须先激活自己的Render Context。
            MakeCurrent();

            //Do some cleaning work
        }

        public void Close()
        {
            if (m_Closed)
                return;

            closeOpenGL();            

            //glContext.Dispose();
            //glContext = null;
            //glWindow.DestroyWindow();
            this.Dispose();

            m_Closed = true;
        }
        
        public double GetAcurateTime(System.Diagnostics.Stopwatch sw)
        {
            long ticks = sw.ElapsedTicks;
            double freq = (double)System.Diagnostics.Stopwatch.Frequency;
            return 1000.0 * ticks / freq;
        }
        
        public void Render()
        {

            m_Generator.Update(4);

            GL.Clear(ClearBufferMask.ColorBufferBit);


            GL.Begin(BeginMode.Quads);
                
            foreach (GLCallback rectCall in this.rects)
            {
            
                GL.Color3(rectCall.color);

                Rectangle rect = rectCall.rect;
                GL.Vertex2(rect.X, rect.Y);
                GL.Vertex2(rect.X + rect.Width, rect.Y);
                GL.Vertex2(rect.X + rect.Width, rect.Y+rect.Height);
                GL.Vertex2(rect.X , rect.Y+rect.Height);
            }
            GL.End();

        }

        gl1.TransformGenerator m_Generator;

        List<GLCallback> rects = new List<GLCallback>();

        class GLCallback :gl1.TransformGenerator.ObjectCallback{
            MyGLControl outer;

            public Rectangle rect = new Rectangle(-10, -10, 20, 20);

            public double[] color={1,1,1};

            public GLCallback(MyGLControl control) {
                outer = control;

                outer.rects.Add(this);

                color[0] = random.NextDouble();
                color[1] = random.NextDouble();
                color[2] = random.NextDouble();
            }

            static Random random=new Random();

            public void OnUpdate(int trackIndex,float scale, float rotate, float r,float globalLength) {

                float scale1 = Math.Min(1,r / 200);
                rect.Width = (int)(200* scale1);
                rect.Height= (int)(200* scale1);

                rect.X = (int)(r * Math.Cos(rotate / 180 * 3.1415926)-rect.Width/2);
                rect.Y = (int)(r * Math.Sin(rotate / 180 * 3.1415926)-rect.Height/2);

                Console.WriteLine(rect.ToString());

            }

            public void OnEnd(int trackIndex) {
                outer.rects.Remove(this);
            }
        }


        System.Timers.Timer timer = new System.Timers.Timer();
        public MyGLControl()
        {
            m_Generator = new gl1.TransformGenerator(() => { return new GLCallback(this); },
                6,60);


            timer.Elapsed += (object source, System.Timers.ElapsedEventArgs arg) => { this.Invalidate(); };
            timer.Interval = 30;
            timer.Enabled = true;
        }
        
    }

        }
    }