using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfApp
{
    class Message
    {
        private string text;

        private bool isAnswer;

        private HorizontalAlignment alignment;

        public Message(string text, bool isAnswer)
        {
            this.text = text;
            this.isAnswer = isAnswer;

            if (isAnswer)
            {
                alignment = HorizontalAlignment.Right;
            }
            else
            {
                alignment = HorizontalAlignment.Left;
            }
        }

        public string Text
        {
            get
            {
                return this.text;
            }
        }

        public HorizontalAlignment Alignment
        {
            get
            {
                return this.alignment;
            }
        }
    }
}
