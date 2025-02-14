using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class drawline : RawImage
{
    //存线的所有四边形,注意，其坐标以最中心为0，0，将ui的坐标集体往右上方平移即可
    private List<List<UIVertex>> rectlist = new List<List<UIVertex>>();
    public int width = 100;//线的长度
    protected override void OnPopulateMesh(VertexHelper vh)
    {
        base.OnPopulateMesh(vh);
        //画弧线
        for(int i = 0; i < rectlist.Count; i++)
        {


            vh.AddUIVertexQuad(rectlist[i].ToArray());


           // Debug.Log("hhh");
        }
        
        List<List<UIVertex>> list = new List<List<UIVertex>>();
        List<UIVertex> t = new List<UIVertex>();
        UIVertex uIVertex = new UIVertex();
       // 
        uIVertex.color = Color.red;
        uIVertex.position = new Vector3(0,200,0);
        uIVertex.position = new Vector3(-86.22f, -71.87f, 0.00f);
        t.Add(uIVertex);
            UIVertex leftDown = new UIVertex();
        leftDown.position = new Vector3(0, 0, 0);
        leftDown.position = new Vector3(-69.05f, -61.94f, 0.00f);
        leftDown.color = Color.red;
        t.Add(leftDown); 
        UIVertex rightDown = new UIVertex();
        rightDown.position = new Vector3(200, 0, 0);
        rightDown.position = new Vector3(-84.93f, -74.09f, 0.00f);
        rightDown.color = Color.red;
        t.Add(rightDown); /* */
        
        UIVertex rightUP = new UIVertex();
        rightUP.position = new Vector3(200, 200, 0);
        rightUP.position = new Vector3(-69.03f, -61.96f, 0.00f);
        rightUP.color = Color.red;
        t.Add(rightUP);vh.AddUIVertexQuad(t.ToArray());
       
    }
    Vector3 last;
    int first = 0;
    public void addRect(Vector2 xy)
    {
        width = 1;
        if(first == 0)
        {
            last = new Vector3(xy.x, xy.y, 0);
            first = 1;
            return;
        }
        Vector3 now = new Vector3(xy.x, xy.y, 0);
        Vector3 direct = (now - last).normalized;
        Vector3 vertical = Vector3.Cross(direct, Vector3.forward).normalized;
     //   Debug.Log("vertical: " + vertical);
        List<UIVertex> t = new List<UIVertex>();
        UIVertex leftUp = new UIVertex();
        leftUp.position = last + vertical * width;
       // Debug.Log(leftUp.position);
      
        leftUp.color = Color.red;
        t.Add(leftUp);

        UIVertex leftDown = new UIVertex();
        leftDown.position = last - vertical * width;
        leftDown.color = Color.red;
       // Debug.Log(meshOrigin.position);
        t.Add(leftDown);        

        UIVertex rightDown = new UIVertex();
        rightDown.position = now - vertical * width;
        rightDown.color = Color.red;
        t.Add(rightDown);
        //Debug.Log(rightDown.position);

        UIVertex rightUP = new UIVertex();
        rightUP.position = now + vertical * width;
        rightUP.color = Color.red;
        t.Add(rightUP);
        //Debug.Log(rightUP.position);
        rectlist.Add(t);
        last = now;

        SetVerticesDirty();
    }
 /*   
    protected override void Start()
    {
        mtw = GameObject.Find("MoveThroughWay").GetComponent<MoveThroughWay>();
    }
    
    private void Update()
    {
        while(mtw.toGOUI.Count > 0)
        {

            Vector3 v = new mtw.toGOUI[0]

            List<UIVertex> vertex = new List<UIVertex>();
            UIVertex uIVertex = new UIVertex();
            uIVertex.position = lastUP;

            UIVertex meshOrigin = new UIVertex();
            meshOrigin.position = lastDown;

            UIVertex rightUP = new UIVertex();
            rightUP.position = lastUP;

            UIVertex rightDown = new UIVertex();
            rightDown.position = lastUP;


        }

    }*/
}
