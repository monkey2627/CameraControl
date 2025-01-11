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
		double[,] Mat = new double[exhibits.transform.childCount, exhibits.transform.childCount];   //�����ά����
		for (int i = 0; i < exhibits.transform.childCount; i++)
		{
			for (int j = 0; j < exhibits.transform.childCount; j++)
			{
				Mat[i, j] = (exhibits.transform.GetChild(i).transform.position - exhibits.transform.GetChild(j).transform.position).magnitude;
			}
		}

		//ע�⣬�������Ч�����õĻ����ֶ��趨·��������Astar����




		//�õ�������Ʒ�Ĵֲ�·��,��0��ʼ����СΪexhibits.transform.childCount
		int[] order = ga.GaTsp(Mat,exhibits.transform.childCount);
		//���ݱ���˳����Astar�㷨�õ�·��
		GameObject begin = exhibits.transform.GetChild(order[0]).gameObject;
		Vector2 last = new Vector2(begin.transform.position.x, begin.transform.position.z);
		//��ֲڻ�·�����еĹؼ���
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
		//β�͵���㣬�γ�һ����·
		thisPoints =  Astar.instance.FindPath(last, new Vector2(begin.transform.position.x, begin.transform.position.z));
		foreach (Point p in thisPoints)
		{
			points.Add(p);
		}
		return points;
	}

}

class Ga
{	/* ������TSP������Ŵ��㷨ʵ���� */
	private Floyd minfloyd = new Floyd();//����Floyd��
	private double[,] Distance;
	private int N; //Ⱥ���ģ
	private int Length; //���峤��
	private double Pc; //�������
	private double Pm; //�������
	private int MaxGene; //����������
	public int[] MinIndividual; //��ǰ������ø���ָ��
	public double MinValue; //����ǰ������ø������Ӧ��ֵ
	private int[,] Buf;  //Ⱥ�����
	private int[,] Buf1; //�м�Ⱥ�����
	private int[,] Buf2; //�м�Ⱥ�����
	private double[] FitV; //Ⱥ��Buf��ÿ���������Ӧ��
	private double[] FitV1;//Ⱥ��Buf1��ÿ���������Ӧ��
	private double[] FitV2;//Ⱥ��Buf2��ÿ���������Ӧ��
	private int[] ppindivi; //�����������õ�����ת����
	private int[] pp;//�����������õ�����ת����
	private RandNumber randnumber = new RandNumber();//����һ���������
													 //���ʼ������
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
	//���ʼ������
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
	//���ص�i���������Ӧ�ȣ�order==0,����Buf��;order==1,����Buf1��;���򷵻�Buf2�ģ�
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
	//˳�򽻲����ӡ���˫��Buf�еĵ�i1��j1�������彻���õ��ĺ������Buf1�еĵ�k1������
	public void CrossOX(int i1, int j1, int k1)
	{
		/* ����[0��Length-1]������������������ĸ���l1,С�ĸ���l2��
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
		//����l1��l2�ĸ�ֵ

		//��Buf�ĵ�j1�����帳����ת����ppindivi
		for (int i = 0; i < Length; i++)
			ppindivi[i] = Buf[j1, i];
		//���pp������pp[ii]��ʾ����ii�ڸ���ppindivi�е�λ��
		for (int i = 0; i < Length; i++)
			pp[ppindivi[i]] = i;
		//��Buf�еĵ�i1�������е�l1��l2֮��Ķ����ڵ�i2����������-1��ǳ���
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
	//��ת��������
	//��Buf1�еĵ�i1��������з�ת����
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
	//������ѡ������
	//��Buf��Buf1��ѡ������Buf2�У������ת��Buf��
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
	//�ҳ���ǰ��ø��壬������棬���Buf��Ѱ�ң����򣬴�Buf��Buf1��Ѱ��
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
	//�������к���
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
	//TSP������Ŵ��㷨��������
	/*public void GaTsp(GameObject exhibits)
	{
		//��ʼ������
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
	//TSP������Ŵ��㷨��������
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
	//��ø����·��,����㿪ʼ
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
	private System.Random rr = new System.Random(); //����Random���ʵ��
									  // ����low��high֮���number���������������low��high��
	public int[] RandInt(int low, int high, int number)
	{
		int[] randvec;
		randvec = new int[number];
		for (int i = 0; i < number; i++)
			randvec[i] = rr.Next() % (high - low + 1) + low;
		return randvec;
	}
	// ����0��high֮���number���������������0��high��
	public int[] RandInt(int high, int number)
	{
		return RandInt(0, high, number);
	}
	// ����low��high֮���number(numberС��high-low+1������ͬ���������������low��high��
	public int[] RandDifferInt(int low, int high, int number)
	{
		if (number > (high - low + 1)) number = high - low + 1;
		int[] randvec;
		randvec = new int[number];
		randvec[0] = rr.Next() % (high - low + 1) + low;
		int randi;  //�洢�м���̲������������
		bool IsDiffer;//�����ж��²�������������Ƿ�����ǰ��������ͬ������ͬΪ�棬Ϊ��
		for (int i = 1; i < randvec.GetLength(0); i++)
		{
			while (true)
			{
				randi = rr.Next() % (high - low + 1) + low;
				IsDiffer = true;   //�趨Ϊ��
				for (int j = 0; j < i; j++)
				{
					if (randi == randvec[j])
					{
						IsDiffer = false; //��ͬΪ��
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
	// ����low��high֮���high-low+1����ͬ���������������low��high��
	public int[] RandDifferInt(int low, int high)
	{
		return RandDifferInt(low, high, high - low + 1);
	}
	// ����0��high֮���high+1����ͬ���������������0��high��
	public int[] RandDifferInt(int high)
	{
		return RandDifferInt(0, high, high + 1);
	}
	// ����һ��[0��1]֮��ķ��Ӿ��ȷֲ��������
	public double Rand01()
	{
		return rr.NextDouble();
	}
	// ����number��[0��1]֮��ķ��Ӿ��ȷֲ��������
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
	private double[,] Distance; //ͼ�����·������
	private int[,] Path; //ͼ�����·���Ľ�ǰ�������
	public int GetDotN()  //��ö�����
	{
		return Path.GetLength(0);
	}
	public double GetDistance(int i, int j)  //��ö���i������j����̾���
	{
		return Distance[i, j];
	}
	//���ͼ�����·������
	public double[,] GetDistance()
	{
		return Distance;
	}
	//��ö���ni������nj�����·��
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
		//�õ�cost����
		CostMat = LSMat.LoadDoubleDataFile(exhibits);
		MinFloyd(CostMat);
	}


	//Floyd�㷨��ʵ��
	//CostMat��ͼ��Ȩֵ����
	public void MinFloyd(double[,] CostMat)
	{
		int nn;
		nn = CostMat.GetLength(0);  //���ͼ�Ķ������
		for (int i = 0; i < nn; i++)
			for (int j = 0; j < nn; j++)
			{
				if (CostMat[i, j] == 0)
					CostMat[i, j] = 10e+100;
			}
		Distance = new double[nn, nn];
		Path = new int[nn, nn];
		//��ʼ��Path,Distance
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
	//�������ļ��л�ȡ��ά���󣬾��������ÿ����֮��ľ���
	public static double[,] LoadDoubleDataFile(GameObject exhibits)
	{
		double[,] Mat = new double[exhibits.transform.childCount,exhibits.transform.childCount];   //�����ά����
		for (int i = 0; i < exhibits.transform.childCount; i++)
		{
			for (int j = 0; j < exhibits.transform.childCount; j++)
			{
				Mat[i,j] =  (exhibits.transform.GetChild(i).transform.position - exhibits.transform.GetChild(j).transform.position).magnitude;
			}
		}
		return Mat;  //���ض�ά����
	}

	private static double[,] LoadDoubleDataFileFormat2(StreamReader r)
	{
		string str;
		str = r.ReadLine();  //���������ļ��ĵڶ���
		int seek = str.IndexOf("matrix");
		str = str.Remove(seek, 6);
		double[] rowcolv;  //����һά����
		rowcolv = StringToDouble(str, 2); //���ض��ַ���ת��Ϊһά����
		double[,] Mat;   //�����ά����
		Mat = new double[(int)rowcolv[0], (int)rowcolv[1]]; //������ά����Mat
		double[] rowvalue = new double[Mat.GetLength(1)];
		int rowi = 0;
		while (r.Peek() > -1)    //�����������ļ�βʱ����ѭ��
		{
			str = r.ReadLine();  //���������ļ���ǰ���ַ���
			int seekm = str.IndexOf(":");
			str = str.Remove(0, seekm + 1);
			rowvalue = StringToDouble(str, Mat.GetLength(1));//���ض��ַ���ת��Ϊһά����
			for (int i = 0; i < Mat.GetLength(1); i++)
				Mat[rowi, i] = rowvalue[i];
			rowi++;
		}
		return Mat;  //���ض�ά����
	}
	//�������ļ��л�ȡ��ά����
	public static int[,] LoadIntleDataFile(string DataFileName)
	{
		FileStream fs;  //�ļ�ָ��
		fs = File.Open(DataFileName, FileMode.Open, FileAccess.Read); //��ֻ����ʽ�������ļ�
		StreamReader r = new StreamReader(fs);//����������
		string str;
		str = r.ReadLine();  //���������ļ��ĵ�һ��
		int[] vv;  //����һά����
		vv = StringToInt(str, 2); //���ض��ַ���ת��Ϊһά����
		int[,] Mat;
		switch (vv[0])
		{
			case 1: Mat = LoadIntleDataFileFormat1(r); break;
			case 2: Mat = LoadIntleDataFileFormat2(r); break;
			default: Mat = new int[0, 0]; break;
		}
		r.Close();
		fs.Close();  //�ر������ļ�ָ��
		return Mat;  //���ض�ά����
	}
	private static int[,] LoadIntleDataFileFormat1(StreamReader r)
	{
		string str;
		str = r.ReadLine();  //���������ļ��ĵڶ���
		int seek = str.IndexOf("matrix");
		Console.WriteLine("{0}", seek);
		str = str.Remove(seek, 6);
		int[] rowcolv;  //����һά����
		rowcolv = StringToInt(str, 1); //���ض��ַ���ת��Ϊһά����
		int[,] Mat;   //�����ά����
		Mat = new int[rowcolv[0], rowcolv[1]]; //������ά����Mat
		str = r.ReadLine(); //���������ļ��ĵ����У��ڴ�û�ã�ԭ�����չ˵�������Եļ�����
		while (r.Peek() > -1)    //�����������ļ�βʱ����ѭ��
		{
			str = r.ReadLine();  //���������ļ���ǰ���ַ���
			rowcolv = StringToInt(str, Mat.GetLength(1));//���ض��ַ���ת��Ϊһά����
			Mat[rowcolv[0] - 1, rowcolv[1] - 1] = rowcolv[2]; //Ϊ��ά���鸳ֵ
		}
		return Mat;  //���ض�ά����
	}
	private static int[,] LoadIntleDataFileFormat2(StreamReader r)
	{
		string str;
		str = r.ReadLine();  //���������ļ��ĵڶ���
		int seek = str.IndexOf("matrix");
		str = str.Remove(seek, 6);
		double[] rowcolv;  //����һά����
		rowcolv = StringToDouble(str, 2); //���ض��ַ���ת��Ϊһά����
		int[,] Mat;   //�����ά����
		Mat = new int[(int)rowcolv[0], (int)rowcolv[1]]; //������ά����Mat
		int[] rowvalue = new int[Mat.GetLength(1)];
		int rowi = 0;
		while (r.Peek() > -1)    //�����������ļ�βʱ����ѭ��
		{
			str = r.ReadLine();  //���������ļ���ǰ���ַ���
			int seekm = str.IndexOf(":");
			str = str.Remove(0, seekm + 1);
			rowvalue = StringToInt(str, Mat.GetLength(1));//���ض��ַ���ת��Ϊһά����
			for (int i = 0; i < Mat.GetLength(1); i++)
				Mat[rowi, i] = rowvalue[i];
			rowi++;
		}
		return Mat;  //���ض�ά����
	}
	//��˫���������ݾ������ָ���ļ���
	public static void SaveDataFile(string DataFileName, double[,] Mat, int DataFileFormat)
	{
		FileStream fs;//�ļ�ָ��
		fs = File.Open(DataFileName, FileMode.Create, FileAccess.Write);//���������ļ�ָ��
		StreamWriter w = new StreamWriter(fs);//����д���ļ���
		switch (DataFileFormat)
		{
			case 1: SaveDataFileFormat1(w, Mat); break;
			case 2: SaveDataFileFormat2(w, Mat); break;
			default: break;
		}
		w.Close();
		fs.Close();
	}
	//��˫���������ݾ��������ݸ�ʽ1�����ļ���ָ����ļ���
	private static void SaveDataFileFormat1(StreamWriter w, double[,] Mat)
	{
		w.WriteLine("1");
		string str;//�����ַ���
		string str2 = " ";//����һ���ո��ַ���
		str = "matrix " + Mat.GetLength(0).ToString() + str2 + Mat.GetLength(1);// �к����ַ���
		w.WriteLine(str);//д����ַ���
						 //�������Ԫ�صĸ���
		int unzeronumber = 0;
		for (int i = 0; i < Mat.GetLength(0); i++)
			for (int j = 0; j < Mat.GetLength(1); j++)
			{
				if (Mat[i, j] != 0.0) unzeronumber++;
			}
		w.WriteLine("{0}", unzeronumber.ToString());
		//��ÿ��Ԫ��д���ļ�
		for (int i = Mat.GetLowerBound(0); i <= Mat.GetUpperBound(0); i++)
			for (int j = Mat.GetLowerBound(1); j <= Mat.GetUpperBound(1); j++)
			{
				str = (i + 1).ToString() + str2 + (j + 1).ToString() + str2 + Mat[i, j].ToString();
				w.WriteLine(str);
			}
	}
	//��˫���������ݾ��������ݸ�ʽ2�����ļ���ָ����ļ���
	private static void SaveDataFileFormat2(StreamWriter w, double[,] Mat)
	{
		w.WriteLine("2");
		string str;//�����ַ���
		string str2 = " ";//����һ���ո��ַ���
		str = "matrix " + Mat.GetLength(0).ToString() + str2 + Mat.GetLength(1);// �к����ַ���
		w.WriteLine(str);//д����ַ���
						 //��ÿ��Ԫ��д���ļ�
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
	//���������ݾ������ָ���ļ���
	public static void SaveDataFile(string DataFileName, int[,] Mat, int DataFileFormat)
	{
		FileStream fs;//�ļ�ָ��
		fs = File.Open(DataFileName, FileMode.Create, FileAccess.Write);//���������ļ�ָ��
		StreamWriter w = new StreamWriter(fs);//����д���ļ���
		switch (DataFileFormat)
		{
			case 1: SaveDataFileFormat1(w, Mat); break;
			case 2: SaveDataFileFormat2(w, Mat); break;
			default: break;
		}
		w.Close();
		fs.Close();
	}
	//��˫���������ݾ��������ݸ�ʽ1�����ļ���ָ����ļ���
	private static void SaveDataFileFormat1(StreamWriter w, int[,] Mat)
	{
		w.WriteLine("1");
		string str;//�����ַ���
		string str2 = " ";//����һ���ո��ַ���
		str = "matrix " + Mat.GetLength(0).ToString() + str2 + Mat.GetLength(1);// �к����ַ���
		w.WriteLine(str);//д����ַ���
						 //�������Ԫ�صĸ���
		int unzeronumber = 0;
		for (int i = 0; i < Mat.GetLength(0); i++)
			for (int j = 0; j < Mat.GetLength(1); j++)
			{
				if (Mat[i, j] != 0.0) unzeronumber++;
			}
		w.WriteLine("{0}", unzeronumber.ToString());
		//��ÿ��Ԫ��д���ļ�
		for (int i = Mat.GetLowerBound(0); i <= Mat.GetUpperBound(0); i++)
			for (int j = Mat.GetLowerBound(1); j <= Mat.GetUpperBound(1); j++)
			{
				str = (i + 1).ToString() + str2 + (j + 1).ToString() + str2 + Mat[i, j].ToString();
				w.WriteLine(str);
			}
	}
	//��˫���������ݾ��������ݸ�ʽ2�����ļ���ָ����ļ���
	private static void SaveDataFileFormat2(StreamWriter w, int[,] Mat)
	{
		w.WriteLine("2");
		string str;//�����ַ���
		string str2 = " ";//����һ���ո��ַ���
		str = "matrix " + Mat.GetLength(0).ToString() + str2 + Mat.GetLength(1);// �к����ַ���
		w.WriteLine(str);//д����ַ���
						 //��ÿ��Ԫ��д���ļ�
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
	//��ֻ���пո����С�����"0-9"���ַ����ַ���ת��Ϊһ��һά˫����������
	private static double[] StringToDouble(string str, int veclength)
	{
		double[] Mat; //����һά����
		Mat = new Double[veclength];  //����һά����
		string str1 = null;   //�������ַ���
		int index1 = 0;  //����һ������������0
		if (str[0] == ' ')  //����ַ��������ַ�Ϊ��
		{
			//��ѭ��Ŀ�����ҵ���һ����Ϊ"�ո�"���ַ�������������������index1
			for (int i = 1; i < str.Length; i++)
			{
				if (str[i] != ' ')
				{
					index1 = i;
					break;
				}
			}

		}
		//����ѭ����Ŀ�����Կո�Ϊ�ֽ���ҳ��������ַ�����ͬʱ����Щ���ַ���ת����˫���ȸ�����
		int k = 0;//��־��Ŀǰ�����ַ�������ţ���0��ʼ����
		for (int i = index1; i < str.Length; i++) //�ӵ�һ����"�ո�"�ַ���ʼ
		{
			if (str[i] == ' ' || i >= (str.Length - 1)) //�����i���ַ�Ϊ"�ո�"�����ѵ��ַ�����β
			{
				//����ѵ��ַ�����β���Ҹ��ַ���Ϊ"�ո�"����������ַ��ӵ��������ַ�����
				if (i >= (str.Length - 1) && (str[i] != ' '))
					str1 += str[i];
				Mat[k] = double.Parse(str1);//ת���ַ���Ϊ˫���ȸ�����
				if (i < (str.Length - 1))  //���δ���ַ�����β
				{
					k++;  //��������һ�����ַ���
					str1 = str1.Remove(0, str1.Length); //������ַ���
														//����ѭ����Ŀ���Ǵӵ�ǰ���ַ�����β��"�ո�"��ʼֱ���ҵ���һ����"�ո�"�ַ�������
														//���ߵ��ַ�����β������֪�������ݼ�����ж���ո�
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
				str1 += str[i];//���������"�ո�"�ַ�����ǰ���ַ���
			}
		}
		return Mat; //�����ַ�����Ӧ��һά����
	}
	//��ֻ���пո����"0-9"���ֵ��ַ���ת��Ϊһ��һά��������
	private static int[] StringToInt(string str, int veclength)
	{
		int[] Mat; //����һά����
		Mat = new int[veclength];  //����һά����
		string str1 = null;   //�������ַ���
		int index1 = 0;  //����һ������������0
		if (str[0] == ' ')  //����ַ��������ַ�Ϊ��
		{
			//��ѭ��Ŀ�����ҵ���һ����Ϊ"�ո�"���ַ�������������������index1
			for (int i = 1; i < str.Length; i++)
			{
				if (str[i] != ' ')
				{
					index1 = i;
					break;
				}
			}

		}
		//����ѭ����Ŀ�����Կո�Ϊ�ֽ���ҳ��������ַ�����ͬʱ����Щ���ַ���ת����˫���ȸ�����
		int k = 0;//��־��Ŀǰ�����ַ�������ţ���0��ʼ����
		for (int i = index1; i < str.Length; i++) //�ӵ�һ����"�ո�"�ַ���ʼ
		{
			if (str[i] == ' ' || i >= (str.Length - 1)) //�����i���ַ�Ϊ"�ո�"�����ѵ��ַ�����β
			{
				//����ѵ��ַ�����β���Ҹ��ַ���Ϊ"�ո�"����������ַ��ӵ��������ַ�����
				if (i >= (str.Length - 1) && (str[i] != ' '))
					str1 += str[i];
				Mat[k] = int.Parse(str1);//ת���ַ���Ϊ˫���ȸ�����
				if (i < (str.Length - 1))  //���δ���ַ�����β
				{
					k++;  //��������һ�����ַ���
					str1 = str1.Remove(0, str1.Length); //������ַ���
														//����ѭ����Ŀ���Ǵӵ�ǰ���ַ�����β��"�ո�"��ʼֱ���ҵ���һ����"�ո�"�ַ�������
														//���ߵ��ַ�����β������֪�������ݼ�����ж���ո�
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
				str1 += str[i];//���������"�ո�"�ַ�����ǰ���ַ���
			}
		}
		return Mat; //�����ַ�����Ӧ��һά����
	}
}