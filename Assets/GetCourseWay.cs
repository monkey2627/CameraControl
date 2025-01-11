using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GetCourseWay : MonoBehaviour
{
    public GameObject exhibits;
	public List<Point> getOrder()
    {
        Ga ga = new Ga();
		double[,] Mat = new double[exhibits.transform.childCount, exhibits.transform.childCount];   //定义二维数组
		for (int i = 0; i < exhibits.transform.childCount; i++)
		{
			for (int j = 0; j < exhibits.transform.childCount; j++)
			{
				Mat[i, j] = (exhibits.transform.GetChild(i).transform.position - exhibits.transform.GetChild(j).transform.position).magnitude;
			}
		}

		//注意，如果觉得效果不好的话，手动设定路径，交给Astar生成




		//得到遍历藏品的粗糙路径,从0开始，大小为exhibits.transform.childCount
		int[] order = ga.GaTsp(Mat,exhibits.transform.childCount);
		//根据遍历顺序，用Astar算法得到路径
		GameObject begin = exhibits.transform.GetChild(order[0]).gameObject;
		Vector2 last = new Vector2(begin.transform.position.x, begin.transform.position.z);
		//存粗糙回路上所有的关键点
		List<Point> points = new List<Point>();
		List<Point> thisPoints;
		for (int c = 1; c < order.Length; c++)
		{
			GameObject t = exhibits.transform.GetChild(order[c]).gameObject;
			Vector2 next = new Vector2(t.transform.position.x, t.transform.position.z);
			thisPoints = Astar.instance.FindPath(last, next);
			foreach (Point p in thisPoints)
            {
				points.Add(p);
            }
			last = next;
		}
		//尾巴到起点，形成一个回路
		thisPoints =  Astar.instance.FindPath(last, new Vector2(begin.transform.position.x, begin.transform.position.z));
		foreach (Point p in thisPoints)
		{
			points.Add(p);
		}
		return points;
	}

}

class Ga
{	/* 下面是TSP问题的遗传算法实现类 */
	private Floyd minfloyd = new Floyd();//创建Floyd类
	private double[,] Distance;
	private int N; //群体规模
	private int Length; //个体长度
	private double Pc; //交叉概率
	private double Pm; //变异概率
	private int MaxGene; //最大迭代代数
	public int[] MinIndividual; //当前代的最好个体指针
	public double MinValue; //到当前代至最好个体的适应度值
	private int[,] Buf;  //群体矩阵
	private int[,] Buf1; //中间群体矩阵
	private int[,] Buf2; //中间群体矩阵
	private double[] FitV; //群体Buf的每个个体的适应度
	private double[] FitV1;//群体Buf1的每个个体的适应度
	private double[] FitV2;//群体Buf2的每个个体的适应度
	private int[] ppindivi; //交叉算子中用到的中转向量
	private int[] pp;//交叉算子中用到的中转向量
	private RandNumber randnumber = new RandNumber();//创建一个随机数类
													 //类初始化函数
	public void Initialize(GameObject exhibits, int N1, double Pc1, double Pm1, int MaxGene1)
	{
		minfloyd.MinFloyd(exhibits);
		Distance = minfloyd.GetDistance();
		for (int i = 0; i < Distance.GetLength(0); i++)
			Distance[i, i] = 0;
		N = N1;
		Length = Distance.GetLength(0);
		Pc = Pc1;
		Pm = Pm1;
		MaxGene = MaxGene1;
		MinValue = double.MaxValue;
		Buf = new int[N, Length];
		Buf1 = new int[N, Length];
		Buf2 = new int[N, Length];
		FitV = new double[N];
		FitV1 = new double[N];
		FitV2 = new double[N];
		MinIndividual = new int[Length];
		int[] individual;
		for (int i = 0; i < N; i++)
		{
			individual = randnumber.RandDifferInt(Length - 1);
			for (int j = 0; j < Length; j++)
				Buf[i, j] = individual[j];
			FitV[i] = Fit(i, 0);
		}
		ppindivi = new int[Length];
		pp = new int[Length];
	}
	//类初始化函数
	public void Initialize(double[,] CostMat, int N1, double Pc1, double Pm1, int MaxGene1)
	{
		minfloyd.MinFloyd(CostMat);
		Distance = minfloyd.GetDistance();
		for (int i = 0; i < Distance.GetLength(0); i++)
			Distance[i, i] = 0;
		N = N1;
		Length = Distance.GetLength(0);
		Pc = Pc1;
		Pm = Pm1;
		MaxGene = MaxGene1;
		MinValue = double.MaxValue;
		Buf = new int[N, Length];
		Buf1 = new int[N, Length];
		Buf2 = new int[N, Length];
		FitV = new double[N];
		FitV1 = new double[N];
		FitV2 = new double[N];
		MinIndividual = new int[Length];
		int[] individual;
		for (int i = 0; i < N; i++)
		{
			individual = randnumber.RandDifferInt(Length - 1);
			for (int j = 0; j < Length; j++)
				Buf[i, j] = individual[j];
			FitV[i] = Fit(i, 0);
		}
		ppindivi = new int[Length];
		pp = new int[Length];
	}
	//返回第i个个体的适应度（order==0,返回Buf的;order==1,返回Buf1的;否则返回Buf2的）
	public double Fit(int i, int order)
	{
		if (order == 0)
		{
			double fitv = 0.0;
			for (int j = 0; j < Length - 1; j++)
				fitv += Distance[Buf[i, j], Buf[i, j + 1]];
			fitv += Distance[Buf[i, Length - 1], Buf[i, 0]];
			return fitv;
		}
		if (order == 1)
		{
			double fitv = 0.0;
			for (int j = 0; j < Length - 1; j++)
				fitv += Distance[Buf1[i, j], Buf1[i, j + 1]];
			fitv += Distance[Buf1[i, Length - 1], Buf1[i, 0]];
			return fitv;
		}
		else
		{
			double fitv = 0.0;
			for (int j = 0; j < Length - 1; j++)
				fitv += Distance[Buf2[i, j], Buf2[i, j + 1]];
			fitv += Distance[Buf2[i, Length - 1], Buf2[i, 0]];
			return fitv;
		}
	}
	//顺序交叉算子。由双亲Buf中的第i1和j1两个个体交叉后得到的后代赋给Buf1中的第k1个个体
	public void CrossOX(int i1, int j1, int k1)
	{
		/* 产生[0，Length-1]间的两个随机整数。大的赋给l1,小的赋给l2。
		 * */
		int[] randi;
		randi = randnumber.RandDifferInt(0, Length - 1, 2);
		int l1, l2;
		if (randi[0] > randi[1])
		{
			l1 = randi[1];
			l2 = randi[0];
		}
		else
		{
			l1 = randi[0];
			l2 = randi[1];
		}
		//结束l1和l2的赋值

		//把Buf的第j1个个体赋给中转向量ppindivi
		for (int i = 0; i < Length; i++)
			ppindivi[i] = Buf[j1, i];
		//获得pp，其中pp[ii]表示顶点ii在个体ppindivi中的位置
		for (int i = 0; i < Length; i++)
			pp[ppindivi[i]] = i;
		//把Buf中的第i1个个体中的l1到l2之间的顶点在第i2个个体中用-1标记出来
		for (int i = l1; i <= l2; i++)
			ppindivi[pp[Buf[i1, i]]] = -1;
		int k = 0;
		for (int i = 0; i < Length; i++)
		{
			if (ppindivi[i] != -1)
			{
				if (k < l1)
				{
					Buf1[k1, k] = ppindivi[i];
					k++;
				}
				else
				{
					Buf1[k1, k + l2 - l1 + 1] = ppindivi[i];
					k++;
				}
			}
		}
		for (int i = l1; i <= l2; i++)
			Buf1[k1, i] = Buf[i1, i];
		FitV1[k1] = Fit(k1, 1);
	}
	//反转变异算子
	//对Buf1中的第i1个个体进行反转变异
	public void MutateReverse(int i1)
	{
		int[] randi;
		randi = randnumber.RandDifferInt(0, Length - 1, 2);
		int l1, l2;
		if (randi[0] > randi[1])
		{
			l1 = randi[1];
			l2 = randi[0];
		}
		else
		{
			l1 = randi[0];
			l2 = randi[1];
		}
		int midl = (l1 + l2) / 2;
		for (int i = l1; i <= midl; i++)
		{
			int ii = Buf1[i1, i];
			Buf1[i1, i] = Buf1[i1, l1 + l2 - i];
			Buf1[i1, l1 + l2 - i] = ii;
		}
		FitV1[i1] = Fit(i1, 1);
	}
	//锦标赛选择算子
	//从Buf和Buf1中选择后存入Buf2中，最后再转入Buf中
	public void SelectMatch()
	{
		int[] randi;
		for (int i = 0; i < N; i++)
		{
			randi = randnumber.RandDifferInt(0, 2 * N - 1, 2);
			if ((randi[0] < N) && (randi[1] < N))
			{
				if (FitV[randi[0]] < FitV[randi[1]])
				{
					for (int j = 0; j < Length; j++)
						Buf2[i, j] = Buf[randi[0], j];
					FitV2[i] = FitV[randi[0]];
				}
				else
				{
					for (int j = 0; j < Length; j++)
						Buf2[i, j] = Buf[randi[1], j];
					FitV2[i] = FitV[randi[1]];
				}
			}
			else if ((randi[0] < N) && (randi[1] >= N))
			{
				if (FitV[randi[0]] < FitV1[randi[1] - N])
				{
					for (int j = 0; j < Length; j++)
						Buf2[i, j] = Buf[randi[0], j];
					FitV2[i] = FitV[randi[0]];
				}
				else
				{
					for (int j = 0; j < Length; j++)
						Buf2[i, j] = Buf1[randi[1] - N, j];
					FitV2[i] = FitV1[randi[1] - N];
				}
			}
			else if ((randi[0] >= N) && (randi[1] < N))
			{
				if (FitV1[randi[0] - N] < FitV[randi[1]])
				{
					for (int j = 0; j < Length; j++)
						Buf2[i, j] = Buf1[randi[0] - N, j];
					FitV2[i] = FitV1[randi[0] - N];
				}
				else
				{
					for (int j = 0; j < Length; j++)
						Buf2[i, j] = Buf[randi[1], j];
					FitV2[i] = FitV[randi[1]];
				}
			}
			else
			{
				if (FitV1[randi[0] - N] < FitV1[randi[1] - N])
				{
					for (int j = 0; j < Length; j++)
						Buf2[i, j] = Buf1[randi[0] - N, j];
					FitV2[i] = FitV1[randi[0] - N];
				}
				else
				{
					for (int j = 0; j < Length; j++)
						Buf2[i, j] = Buf1[randi[1] - N, j];
					FitV2[i] = FitV1[randi[1] - N];
				}
			}
		}
		int[,] Bufm;
		Bufm = Buf;
		Buf = Buf2;
		Buf2 = Bufm;
		double[] FitVm;
		FitVm = FitV;
		FitV = FitV2;
		FitV2 = FitVm;
	}
	//找出当前最好个体，如果是真，则从Buf中寻找，否则，从Buf和Buf1中寻找
	public void FindMinIndividual(bool iszero)
	{
		int ismin = -1;
		for (int i = 0; i < N; i++)
		{
			if (MinValue > FitV[i])
			{
				ismin = i;
				MinValue = FitV[i];
			}
		}
		if (ismin != -1)
		{
			for (int j = 0; j < Length; j++)
			{
				MinIndividual[j] = Buf[ismin, j];
			}
		}
		if (!iszero)
		{
			ismin = -1;
			for (int i = 0; i < N; i++)
			{
				if (MinValue > FitV1[i])
				{
					ismin = i;
					MinValue = FitV1[i];
				}
			}
			if (ismin != -1)
			{
				for (int i = 0; i < Buf.GetLength(1); i++)
					MinIndividual[i] = Buf1[ismin, i];
			}
		}

	}
	//单步运行函数
	public void GaStep()
	{
		int[] randi;
		for (int i = 0; i < N; i++)
		{
			randi = randnumber.RandDifferInt(0, N - 1, 2);
			if (randnumber.Rand01() < Pc)
				CrossOX(randi[0], randi[1], i);
			else
			{
				for (int j = 0; j < Length; j++)
					Buf1[i, j] = Buf[randi[0], j];
				FitV1[i] = FitV[randi[0]];
			}
		}
		for (int i = 0; i < N; i++)
		{
			if (randnumber.Rand01() < Pm)
				MutateReverse(i);
		}
		SelectMatch();
		FindMinIndividual(false);
	}
	//TSP问题的遗传算法的主程序
	/*public void GaTsp(GameObject exhibits)
	{
		//初始化矩阵
		Initialize(exhibits, 10, 0.6, 0.05, 10000);
		for (int i = 0; i < MaxGene; i++)
			GaStep();
		Debug.Log("MinValue="+ MinValue);
		Debug.Log(GetPath(MinIndividual));

	 	Int64 tt = Convert.ToInt64(GetPath(MinIndividual));

		for (int i = 0; i < 10; i++)
		{
			Debug.Log((tt % 10));
			tt = tt / 10;

		}

	}*/
	//TSP问题的遗传算法的主程序
	public int[] GaTsp(double[,] CostMat,int count)
	{
		Initialize(CostMat, 10, 0.6, 0.05, 50000);
		for (int i = 0; i < MaxGene; i++)
		{
			GaStep();
		}
		Debug.Log("MinValue=" + MinValue);
		return GetPath(MinIndividual,0,count);
	}
	//获得个体的路径,从起点开始
	public int[] GetPath(int[] Individual,int start,int count)
	{
		string path = null;
		string str = "-->";
		string subpath;
		for (int i = 1; i < Length; i++)
		{
			if (Individual[i - 1] < 10)
			{
				subpath = minfloyd.GetPath(Individual[i - 1], Individual[i]).Remove(0, 4);
			}
			else if (Individual[i - 1] < 100)
			{
				subpath = minfloyd.GetPath(Individual[i - 1], Individual[i]).Remove(0, 5);
			}
			else
			{
				subpath = minfloyd.GetPath(Individual[i - 1], Individual[i]).Remove(0, 6);
			}
			path += subpath + str;

			//path += subpath ;
		}
		if (Individual[Length - 1] < 10)
		{
			subpath = minfloyd.GetPath(Individual[Length - 1], Individual[0]).Remove(0, 4);
		}
		else if (Individual[Length - 1] < 100)
		{
			subpath = minfloyd.GetPath(Individual[Length - 1], Individual[0]).Remove(0, 5);
		}
		else
		{
			subpath = minfloyd.GetPath(Individual[Length - 1], Individual[0]).Remove(0, 6);
		}
		path += subpath;
		return getRightOrder(path,count) ;
	}
	public int[] getRightOrder(string path,int count)
	{
		int[] ans = new int[count];
		int begin = 0;
		for (int i = 0; i < path.Length; i++)
		{

			if (path[0] == '0')
			{
				begin = 0;
				break;
			}
			if (i != 0 && path[i - 1] == '>' && path[i] == '0')
			{
				begin = i;
				break;
			}
		}
		int num = 0;
		for (int i = begin; i < path.Length; i++)
		{
			if (path[i] >= '0' && path[i] <= '9')
			{
				int t = 0;
				while (i < path.Length && path[i] != '-')
				{
					t = t * 10 + (path[i] - '0');

					i++;
				}
				ans[num++] = t;
				Debug.Log(t);
			}
		}
		for (int i = 0; i < begin; i++)
		{
			if (path[i] >= '0' && path[i] <= '9')
			{
				int t = 0;
				while (i < begin && path[i] != '-')
				{
					t = t * 10 + (path[i] - '0');
					i++;
				}
				ans[num++] = t;
			}
		}
		ans[num++] = 0;
		return ans;
	}
}
class RandNumber
{
	private System.Random rr = new System.Random(); //创建Random类的实例
									  // 返回low与high之间的number个随机整数（包括low和high）
	public int[] RandInt(int low, int high, int number)
	{
		int[] randvec;
		randvec = new int[number];
		for (int i = 0; i < number; i++)
			randvec[i] = rr.Next() % (high - low + 1) + low;
		return randvec;
	}
	// 返回0与high之间的number个随机整数（包括0和high）
	public int[] RandInt(int high, int number)
	{
		return RandInt(0, high, number);
	}
	// 返回low与high之间的number(number小于high-low+1）个不同的随机整数（包括low和high）
	public int[] RandDifferInt(int low, int high, int number)
	{
		if (number > (high - low + 1)) number = high - low + 1;
		int[] randvec;
		randvec = new int[number];
		randvec[0] = rr.Next() % (high - low + 1) + low;
		int randi;  //存储中间过程产生的随机整数
		bool IsDiffer;//用于判断新产生的随机整数是否与以前产生的相同，若不同为真，为假
		for (int i = 1; i < randvec.GetLength(0); i++)
		{
			while (true)
			{
				randi = rr.Next() % (high - low + 1) + low;
				IsDiffer = true;   //设定为真
				for (int j = 0; j < i; j++)
				{
					if (randi == randvec[j])
					{
						IsDiffer = false; //相同为假
						break;
					}
				}
				if (IsDiffer)
				{
					randvec[i] = randi;
					break;
				}
			}
		}
		return randvec;
	}
	// 返回low与high之间的high-low+1个不同的随机整数（包括low和high）
	public int[] RandDifferInt(int low, int high)
	{
		return RandDifferInt(low, high, high - low + 1);
	}
	// 返回0与high之间的high+1个不同的随机整数（包括0和high）
	public int[] RandDifferInt(int high)
	{
		return RandDifferInt(0, high, high + 1);
	}
	// 返回一个[0，1]之间的服从均匀分布的随机数
	public double Rand01()
	{
		return rr.NextDouble();
	}
	// 返回number个[0，1]之间的服从均匀分布的随机数
	public double[] Rand01(int number)
	{
		double[] randf = new double[number];
		for (int i = 0; i < number; i++)
			randf[i] = rr.NextDouble();
		return randf;
	}
}
class Floyd
{
	private double[,] Distance; //图的最短路径矩阵
	private int[,] Path; //图的最短路径的紧前顶点矩阵
	public int GetDotN()  //获得顶点数
	{
		return Path.GetLength(0);
	}
	public double GetDistance(int i, int j)  //获得顶点i到顶点j的最短距离
	{
		return Distance[i, j];
	}
	//获得图的最短路径矩阵
	public double[,] GetDistance()
	{
		return Distance;
	}
	//获得顶点ni到顶点nj的最短路径
	public string GetPath(int ni, int nj)
	{
		int[] pathReverse;
		pathReverse = new int[Path.GetLength(0)];
		pathReverse[0] = Path[ni, nj];
		int k = 0;
		while (pathReverse[k] != ni)
		{
			k++;
			pathReverse[k] = Path[ni, pathReverse[k - 1]];
		}
		string path = pathReverse[k].ToString();
		string str1 = "-->";
		for (int i = k - 1; i >= 0; i--)
		{
			path += (str1 + pathReverse[i].ToString());
		}
		path += (str1 + nj.ToString());
		return path;
	}
	public void MinFloyd(GameObject exhibits)
	{
		double[,] CostMat;
		//得到cost矩阵
		CostMat = LSMat.LoadDoubleDataFile(exhibits);
		MinFloyd(CostMat);
	}


	//Floyd算法的实现
	//CostMat是图的权值矩阵
	public void MinFloyd(double[,] CostMat)
	{
		int nn;
		nn = CostMat.GetLength(0);  //获得图的顶点个数
		for (int i = 0; i < nn; i++)
			for (int j = 0; j < nn; j++)
			{
				if (CostMat[i, j] == 0)
					CostMat[i, j] = 10e+100;
			}
		Distance = new double[nn, nn];
		Path = new int[nn, nn];
		//初始化Path,Distance
		for (int i = 0; i < nn; i++)
		{
			for (int j = 0; j < nn; j++)
			{
				Path[i, j] = i;
				Distance[i, j] = CostMat[i, j];
			}
			Distance[i, i] = 0.0;
		}
		for (int k = 0; k < nn; k++)
			for (int i = 0; i < nn; i++)
				for (int j = 0; j < nn; j++)
				{
					double a1, a2;
					a1 = Distance[i, j];
					a2 = Distance[i, k] + Distance[k, j];
					if (a1 > a2)
					{
						Distance[i, j] = a2;
						Path[i, j] = Path[k, j];
					}
				}
	}
}


class LSMat
{
	//从数据文件中获取二维矩阵，矩阵代表着每个点之间的距离
	public static double[,] LoadDoubleDataFile(GameObject exhibits)
	{
		double[,] Mat = new double[exhibits.transform.childCount,exhibits.transform.childCount];   //定义二维数组
		for (int i = 0; i < exhibits.transform.childCount; i++)
		{
			for (int j = 0; j < exhibits.transform.childCount; j++)
			{
				Mat[i,j] =  (exhibits.transform.GetChild(i).transform.position - exhibits.transform.GetChild(j).transform.position).magnitude;
			}
		}
		return Mat;  //返回二维数组
	}

	private static double[,] LoadDoubleDataFileFormat2(StreamReader r)
	{
		string str;
		str = r.ReadLine();  //读入数据文件的第二行
		int seek = str.IndexOf("matrix");
		str = str.Remove(seek, 6);
		double[] rowcolv;  //定义一维数组
		rowcolv = StringToDouble(str, 2); //把特定字符串转换为一维数组
		double[,] Mat;   //定义二维数组
		Mat = new double[(int)rowcolv[0], (int)rowcolv[1]]; //创建二维数组Mat
		double[] rowvalue = new double[Mat.GetLength(1)];
		int rowi = 0;
		while (r.Peek() > -1)    //当到达数据文件尾时结束循环
		{
			str = r.ReadLine();  //读入数据文件当前行字符串
			int seekm = str.IndexOf(":");
			str = str.Remove(0, seekm + 1);
			rowvalue = StringToDouble(str, Mat.GetLength(1));//把特定字符串转换为一维数组
			for (int i = 0; i < Mat.GetLength(1); i++)
				Mat[rowi, i] = rowvalue[i];
			rowi++;
		}
		return Mat;  //返回二维数组
	}
	//从数据文件中获取二维矩阵
	public static int[,] LoadIntleDataFile(string DataFileName)
	{
		FileStream fs;  //文件指针
		fs = File.Open(DataFileName, FileMode.Open, FileAccess.Read); //以只读方式打开数据文件
		StreamReader r = new StreamReader(fs);//创建读入流
		string str;
		str = r.ReadLine();  //读入数据文件的第一行
		int[] vv;  //定义一维数组
		vv = StringToInt(str, 2); //把特定字符串转换为一维数组
		int[,] Mat;
		switch (vv[0])
		{
			case 1: Mat = LoadIntleDataFileFormat1(r); break;
			case 2: Mat = LoadIntleDataFileFormat2(r); break;
			default: Mat = new int[0, 0]; break;
		}
		r.Close();
		fs.Close();  //关闭数据文件指针
		return Mat;  //返回二维数组
	}
	private static int[,] LoadIntleDataFileFormat1(StreamReader r)
	{
		string str;
		str = r.ReadLine();  //读入数据文件的第二行
		int seek = str.IndexOf("matrix");
		Console.WriteLine("{0}", seek);
		str = str.Remove(seek, 6);
		int[] rowcolv;  //定义一维数组
		rowcolv = StringToInt(str, 1); //把特定字符串转换为一维数组
		int[,] Mat;   //定义二维数组
		Mat = new int[rowcolv[0], rowcolv[1]]; //创建二维数组Mat
		str = r.ReadLine(); //读入数据文件的第三行，在此没用，原因是照顾到别的语言的兼容性
		while (r.Peek() > -1)    //当到达数据文件尾时结束循环
		{
			str = r.ReadLine();  //读入数据文件当前行字符串
			rowcolv = StringToInt(str, Mat.GetLength(1));//把特定字符串转换为一维数组
			Mat[rowcolv[0] - 1, rowcolv[1] - 1] = rowcolv[2]; //为二维数组赋值
		}
		return Mat;  //返回二维数组
	}
	private static int[,] LoadIntleDataFileFormat2(StreamReader r)
	{
		string str;
		str = r.ReadLine();  //读入数据文件的第二行
		int seek = str.IndexOf("matrix");
		str = str.Remove(seek, 6);
		double[] rowcolv;  //定义一维数组
		rowcolv = StringToDouble(str, 2); //把特定字符串转换为一维数组
		int[,] Mat;   //定义二维数组
		Mat = new int[(int)rowcolv[0], (int)rowcolv[1]]; //创建二维数组Mat
		int[] rowvalue = new int[Mat.GetLength(1)];
		int rowi = 0;
		while (r.Peek() > -1)    //当到达数据文件尾时结束循环
		{
			str = r.ReadLine();  //读入数据文件当前行字符串
			int seekm = str.IndexOf(":");
			str = str.Remove(0, seekm + 1);
			rowvalue = StringToInt(str, Mat.GetLength(1));//把特定字符串转换为一维数组
			for (int i = 0; i < Mat.GetLength(1); i++)
				Mat[rowi, i] = rowvalue[i];
			rowi++;
		}
		return Mat;  //返回二维数组
	}
	//把双精度型数据矩阵存入指定文件中
	public static void SaveDataFile(string DataFileName, double[,] Mat, int DataFileFormat)
	{
		FileStream fs;//文件指针
		fs = File.Open(DataFileName, FileMode.Create, FileAccess.Write);//创建并打开文件指针
		StreamWriter w = new StreamWriter(fs);//创建写入文件流
		switch (DataFileFormat)
		{
			case 1: SaveDataFileFormat1(w, Mat); break;
			case 2: SaveDataFileFormat2(w, Mat); break;
			default: break;
		}
		w.Close();
		fs.Close();
	}
	//把双精度型数据矩阵以数据格式1存入文件流指向的文件中
	private static void SaveDataFileFormat1(StreamWriter w, double[,] Mat)
	{
		w.WriteLine("1");
		string str;//定义字符串
		string str2 = " ";//创建一个空格字符串
		str = "matrix " + Mat.GetLength(0).ToString() + str2 + Mat.GetLength(1);// 行和列字符串
		w.WriteLine(str);//写入该字符串
						 //计算非零元素的个数
		int unzeronumber = 0;
		for (int i = 0; i < Mat.GetLength(0); i++)
			for (int j = 0; j < Mat.GetLength(1); j++)
			{
				if (Mat[i, j] != 0.0) unzeronumber++;
			}
		w.WriteLine("{0}", unzeronumber.ToString());
		//把每个元素写入文件
		for (int i = Mat.GetLowerBound(0); i <= Mat.GetUpperBound(0); i++)
			for (int j = Mat.GetLowerBound(1); j <= Mat.GetUpperBound(1); j++)
			{
				str = (i + 1).ToString() + str2 + (j + 1).ToString() + str2 + Mat[i, j].ToString();
				w.WriteLine(str);
			}
	}
	//把双精度型数据矩阵以数据格式2存入文件流指向的文件中
	private static void SaveDataFileFormat2(StreamWriter w, double[,] Mat)
	{
		w.WriteLine("2");
		string str;//定义字符串
		string str2 = " ";//创建一个空格字符串
		str = "matrix " + Mat.GetLength(0).ToString() + str2 + Mat.GetLength(1);// 行和列字符串
		w.WriteLine(str);//写入该字符串
						 //把每个元素写入文件
		for (int i = Mat.GetLowerBound(0); i <= Mat.GetUpperBound(0); i++)
		{
			str = i.ToString() + ":  ";
			w.Write(str);
			for (int j = Mat.GetLowerBound(1); j <= Mat.GetUpperBound(1); j++)
			{
				str = Mat[i, j].ToString() + "  ";
				w.Write(str);
			}
			w.Write("\n");
		}
	}
	//把整型数据矩阵存入指定文件中
	public static void SaveDataFile(string DataFileName, int[,] Mat, int DataFileFormat)
	{
		FileStream fs;//文件指针
		fs = File.Open(DataFileName, FileMode.Create, FileAccess.Write);//创建并打开文件指针
		StreamWriter w = new StreamWriter(fs);//创建写入文件流
		switch (DataFileFormat)
		{
			case 1: SaveDataFileFormat1(w, Mat); break;
			case 2: SaveDataFileFormat2(w, Mat); break;
			default: break;
		}
		w.Close();
		fs.Close();
	}
	//把双精度型数据矩阵以数据格式1存入文件流指向的文件中
	private static void SaveDataFileFormat1(StreamWriter w, int[,] Mat)
	{
		w.WriteLine("1");
		string str;//定义字符串
		string str2 = " ";//创建一个空格字符串
		str = "matrix " + Mat.GetLength(0).ToString() + str2 + Mat.GetLength(1);// 行和列字符串
		w.WriteLine(str);//写入该字符串
						 //计算非零元素的个数
		int unzeronumber = 0;
		for (int i = 0; i < Mat.GetLength(0); i++)
			for (int j = 0; j < Mat.GetLength(1); j++)
			{
				if (Mat[i, j] != 0.0) unzeronumber++;
			}
		w.WriteLine("{0}", unzeronumber.ToString());
		//把每个元素写入文件
		for (int i = Mat.GetLowerBound(0); i <= Mat.GetUpperBound(0); i++)
			for (int j = Mat.GetLowerBound(1); j <= Mat.GetUpperBound(1); j++)
			{
				str = (i + 1).ToString() + str2 + (j + 1).ToString() + str2 + Mat[i, j].ToString();
				w.WriteLine(str);
			}
	}
	//把双精度型数据矩阵以数据格式2存入文件流指向的文件中
	private static void SaveDataFileFormat2(StreamWriter w, int[,] Mat)
	{
		w.WriteLine("2");
		string str;//定义字符串
		string str2 = " ";//创建一个空格字符串
		str = "matrix " + Mat.GetLength(0).ToString() + str2 + Mat.GetLength(1);// 行和列字符串
		w.WriteLine(str);//写入该字符串
						 //把每个元素写入文件
		for (int i = Mat.GetLowerBound(0); i <= Mat.GetUpperBound(0); i++)
		{
			str = i.ToString() + ":  ";
			w.Write(str);
			for (int j = Mat.GetLowerBound(1); j <= Mat.GetUpperBound(1); j++)
			{
				str = Mat[i, j].ToString() + "  ";
				w.Write(str);
			}
			w.Write("\n");
		}
	}
	//把只含有空格符，小数点和"0-9"数字符的字符串转换为一个一维双精度型数组
	private static double[] StringToDouble(string str, int veclength)
	{
		double[] Mat; //定义一维数组
		Mat = new Double[veclength];  //创建一维数组
		string str1 = null;   //创建空字符串
		int index1 = 0;  //创建一个整型数并赋0
		if (str[0] == ' ')  //如果字符串的首字符为空
		{
			//本循环目的是找到第一个不为"空格"的字符的索引，并把它赋给index1
			for (int i = 1; i < str.Length; i++)
			{
				if (str[i] != ' ')
				{
					index1 = i;
					break;
				}
			}

		}
		//下面循环的目的是以空格为分界符找出所有子字符串，同时把这些子字符串转换成双精度浮点数
		int k = 0;//标志着目前的子字符串的序号，从0开始记数
		for (int i = index1; i < str.Length; i++) //从第一个非"空格"字符开始
		{
			if (str[i] == ' ' || i >= (str.Length - 1)) //如果第i个字符为"空格"或者已到字符串结尾
			{
				//如果已到字符串结尾并且该字符不为"空格"，则把最后的字符加到最后的子字符串中
				if (i >= (str.Length - 1) && (str[i] != ' '))
					str1 += str[i];
				Mat[k] = double.Parse(str1);//转换字符串为双精度浮点数
				if (i < (str.Length - 1))  //如果未到字符串结尾
				{
					k++;  //记数到下一个子字符串
					str1 = str1.Remove(0, str1.Length); //清空子字符串
														//下面循环的目的是从当前子字符串结尾的"空格"开始直至找到下一个非"空格"字符的索引
														//或者到字符串结尾（我们知道两数据间可以有多个空格）
					while (true)
					{
						if (str[i + 1] != ' ') break;
						i++;
						if (i >= (str.Length - 1)) break;
					}
				}
			}
			else
			{
				str1 += str[i];//继续加入非"空格"字符到当前子字符串
			}
		}
		return Mat; //返回字符串对应的一维数组
	}
	//把只含有空格符和"0-9"数字的字符串转换为一个一维整型数组
	private static int[] StringToInt(string str, int veclength)
	{
		int[] Mat; //定义一维数组
		Mat = new int[veclength];  //创建一维数组
		string str1 = null;   //创建空字符串
		int index1 = 0;  //创建一个整型数并赋0
		if (str[0] == ' ')  //如果字符串的首字符为空
		{
			//本循环目的是找到第一个不为"空格"的字符的索引，并把它赋给index1
			for (int i = 1; i < str.Length; i++)
			{
				if (str[i] != ' ')
				{
					index1 = i;
					break;
				}
			}

		}
		//下面循环的目的是以空格为分界符找出所有子字符串，同时把这些子字符串转换成双精度浮点数
		int k = 0;//标志着目前的子字符串的序号，从0开始记数
		for (int i = index1; i < str.Length; i++) //从第一个非"空格"字符开始
		{
			if (str[i] == ' ' || i >= (str.Length - 1)) //如果第i个字符为"空格"或者已到字符串结尾
			{
				//如果已到字符串结尾并且该字符不为"空格"，则把最后的字符加到最后的子字符串中
				if (i >= (str.Length - 1) && (str[i] != ' '))
					str1 += str[i];
				Mat[k] = int.Parse(str1);//转换字符串为双精度浮点数
				if (i < (str.Length - 1))  //如果未到字符串结尾
				{
					k++;  //记数到下一个子字符串
					str1 = str1.Remove(0, str1.Length); //清空子字符串
														//下面循环的目的是从当前子字符串结尾的"空格"开始直至找到下一个非"空格"字符的索引
														//或者到字符串结尾（我们知道两数据间可以有多个空格）
					while (true)
					{
						if (str[i + 1] != ' ') break;
						i++;
						if (i >= (str.Length - 1)) break;
					}
				}
			}
			else
			{
				str1 += str[i];//继续加入非"空格"字符到当前子字符串
			}
		}
		return Mat; //返回字符串对应的一维数组
	}
}