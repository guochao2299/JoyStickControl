using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JoyStickControl
{
    public class Player
    {
        private float m_x = 0;
        private float m_y = 0;

        public Player(int x, int y)
        {
            m_x = x;
            m_y = y;
        }

        public void UpdateXPosition(float xSpeed,int minX,int maxX)
        {
            m_x += xSpeed;
            if(m_x<minX)
            {
                m_x=minX;
            }
            if(m_x>maxX)
            {
                m_x=maxX;
            }
        }

        public void UpdateYPosition(float ySpeed,int minY,int maxY)
        {
            m_y += ySpeed;

            if(m_y<minY)
            {
                m_y=minY;
            }
            if(m_y>maxY)
            {
                m_y=maxY;
            }
        }

        public float XPosition
        {
            get
            {
                return m_x;
            }
        }

        public float YPosition
        {
            get
            {
                return m_y;
            }
        }

        public void ResetPosition(int x, int y)
        {
            m_x = x;
            m_y = y;
        }

        private bool m_isPressed = false;
        public bool IsPressed
        {
            get
            {
                return m_isPressed;
            }
            set
            {
                m_isPressed = value;
            }
        }
    }
}
