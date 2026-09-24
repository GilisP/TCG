using System;
using UnityEngine;
using TCG.Table;
public static class PresentationChecks
{
    public static void Run()
    {
        int checks=0;void Check(bool ok,string message){checks++;if(!ok)throw new Exception("PRESENTATION: "+message);}
        Check(PieceFormation.Offset(0,1).x==0&&PieceFormation.Offset(0,1).z==0,"single centered");
        for(int count=2;count<=9;count++)
        {
            Vector3 center=Vector3.zero;for(int i=0;i<count;i++)
            {
                var p=PieceFormation.Offset(i,count);center+=p;
                Check(Mathf.Abs(p.x)+.34f*PieceFormation.Scale(count)<.47f&&Mathf.Abs(p.z)+.34f*PieceFormation.Scale(count)<.47f,"inside tile");
                for(int j=0;j<i;j++)Check(Vector3.Distance(p,PieceFormation.Offset(j,count))>.68f*PieceFormation.Scale(count),"distinct pick areas");
            }
            Check(Mathf.Abs(center.x)<.0001f,"rows centered");
        }
        Check(PieceFormation.Scale(20)==PieceFormation.Scale(9),"overflow presentation stays bounded");
        Debug.Log("PRESENTATION CHECKS PASSED: "+checks);
    }
}
