/**
 * This script creates video tracks whose track motion is preset to
 * create a "Brady Bunch"-style grid.
 *
 * Revision Date: Jul 09, 2007.
 **/

using System;
using Sony.Vegas;
using System.Collections.Generic;

public class EntryPoint
{

    System.IO.StreamWriter file = new System.IO.StreamWriter(@"E:\log.txt");

    public gl1.TransformGenerator generator;

    public Vegas vegas;

    public Dictionary<int,VideoTrack> m_TrackList=new Dictionary<int,VideoTrack>();

    public class TrackCallback : gl1.TransformGenerator.ObjectCallback
    {
        EntryPoint m_Outer;
        public TrackCallback(EntryPoint outer)
        {
            m_Outer = outer;
        }

        public void OnEnd(int trackIndex)
        {
            VideoTrack thisTrack;
            m_Outer.m_TrackList.TryGetValue(trackIndex, out thisTrack);

            TrackMotionKeyframe lastKey = thisTrack.TrackMotion.MotionKeyframes[thisTrack.TrackMotion.MotionKeyframes.Count-1];

            lastKey.Type = VideoKeyframeType.Sharp;
        }

        public void OnUpdate(int trackIndex,float scale, float rotate, float r, float globalLength)
        {
            VideoTrack thisTrack;
            if (!m_Outer.m_TrackList.TryGetValue(trackIndex,out thisTrack))
            {
                thisTrack = new VideoTrack(trackIndex);

                m_Outer.file.WriteLine("add new track"+trackIndex);

                m_Outer.m_TrackList.Add(trackIndex, thisTrack);

                m_Outer.vegas.Project.Tracks.Add(thisTrack);
            }

            TrackMotionKeyframe key=
                thisTrack.TrackMotion.InsertMotionKeyframe(Timecode.FromSeconds(globalLength));

            key.Type = VideoKeyframeType.Smooth;

            key.Width = m_Outer.m_FrameWidth / 5 * scale;
            key.Width = m_Outer.m_FrameWidth / 5 * scale;
                
            m_Outer.file.WriteLine("update track"+trackIndex);
        }
    }

    public TrackCallback creator()
    {
        return new TrackCallback(this);
    }

    public int m_FrameWidth;
    public int m_FrameHeight;

    public void FromVegas(Vegas vegas)
    {
        this.vegas = vegas;

        m_FrameHeight = vegas.Project.Video.Height;
        m_FrameWidth = vegas.Project.Video.Width;

        generator = new gl1.TransformGenerator(this.creator, 6, m_FrameWidth/7);

        for (int i = 1; i < 100; ++i)
        {
            generator.Update(4);
        }
   }
}
