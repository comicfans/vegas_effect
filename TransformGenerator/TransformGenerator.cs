/*
 * Created by SharpDevelop.
 * User: wangxy
 * Date: 2013/7/16
 * Time: 16:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;

namespace gl1
{
	/// <summary>
	/// Description of TransformGenerator.
	/// </summary>
	public class TransformGenerator
	{
		
		public interface ObjectCallback{			
			void OnUpdate(int trackIndex,float scale,float rotate,float r,float globalLength);
			void OnEnd(int trackIndex);
		}
		
		public delegate ObjectCallback ObjectCallbackCreator();
		
		private ObjectCallbackCreator m_Creator;

        private float m_MinCreateInterval;

        private float m_GlobalLength=0;
		
		public TransformGenerator(ObjectCallbackCreator creator,
		                          int branchNumber,float minCreatInterval)
		{
			m_Creator=creator;

            m_MinCreateInterval = minCreatInterval;

            for (int i = 0; i < branchNumber; ++i)
            {
                m_AllBranches.Add(new TrackBranch());
            }

		}

        float m_prevMinR=0;

        int trackCounter = 0;

        public void Update(float interval)
        {
            		
            m_GlobalLength += interval;

            float thisMinR=99999;

            for(int branchCounter=0;branchCounter<m_AllBranches.Count;++branchCounter){

                TrackBranch trackBranch =m_AllBranches[branchCounter];
                int recycleNumber=0;

                for (int i = 0; i < trackBranch.list.Count; ++i) {

                    float thisR = trackBranch.list[i].Update(interval);

                    if (i == 0 )
                    {
                        thisMinR = thisR;
                    }

                    if (thisR > m_MaxR)
                    {
                        trackBranch.list[i].objectCallback.OnEnd(trackBranch.list[i].trackIndex);
                        ++recycleNumber;
                    }
                }

                bool needsNewTrack= thisMinR>m_MinCreateInterval;


                if (!needsNewTrack && (recycleNumber==0))
                {
                    continue;
                }

                int absRecycle = recycleNumber - (needsNewTrack ? 1 : 0);

                    

                if (absRecycle > 0)
                {
                    int lastReserveIndex = trackBranch.list.Count - absRecycle-1;
                    trackBranch.list.RemoveRange(lastReserveIndex+1 , absRecycle);

                    if (needsNewTrack)
                    {
                        VideoTrack track = trackBranch.list[lastReserveIndex];
                        track.Reset(m_Creator());
                    }

                    continue;
                }

                if (needsNewTrack)
                {
                    float angleOffset = (float)360 / m_AllBranches.Count * branchCounter;
                    trackBranch.list.Insert(0, new VideoTrack(trackCounter++,angleOffset,this));
                    trackBranch.list[0].Reset(m_Creator());
                }
            }

            m_prevMinR = thisMinR;

			
		}

        public static float m_MaxR=640;

		public class VideoTrack{

            TransformGenerator m_Outer;

            public VideoTrack(int trackIndexSet, float angleOffsetSet,TransformGenerator outer)
            {
                trackIndex = trackIndexSet;
                angleOffset = angleOffsetSet;
                m_Outer = outer;
            }

            public void Reset(ObjectCallback objectCallbackSet)
            {
                objectCallback = objectCallbackSet;
                angle = angleOffset;
                Update(0);
            }

			public int trackIndex;

            // r(angle)= a*(angle-c)^2+b*(angle-c)
            static float factor2=0.001f;
            static float factor1=0.6f;
            float angle;

            float angleOffset;

            public float Update(float interval)
            {
                angle += interval;

                float finalAngle=angle-angleOffset;

                float r = factor2 * finalAngle * finalAngle + factor1 * finalAngle;

                float scale = Math.Min(1, r/200);

                objectCallback.OnUpdate(trackIndex,scale, angle, r,m_Outer.m_GlobalLength);

                return r;

            }
            
			public ObjectCallback objectCallback;
		}


        public class TrackBranch {
            public List<VideoTrack> list=new List<VideoTrack>();
        }
		
		
		private List<TrackBranch> m_AllBranches=new List<TrackBranch>();
		
		
		
		
	}
}
