using System;
using System.Collections.Generic;
using System.Text;

namespace SP
{
    public class Line
    {
        public string Location { get; set; }
        public string Part1 { get; set; }
        public string Part2 { get; set; }
        public string Part3 { get; set; }
        public string ObjectCode { get; set; }

        public Line(string p1, string p2, string p3)
        {
            Part1 = p1;
            Part2 = p2;
            Part3 = p3;
        }
        public Line()
        {

        }
    }
}
