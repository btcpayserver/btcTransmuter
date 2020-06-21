using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcTransmuter.Abstractions.Helpers;
using Xunit;

namespace BtcTransmuter.Tests
{
    public class InterpolationTests
    {
        [Fact]
        public void CanInterpolate()
        {
            Assert.Equal("hello world",InterpolationHelper.InterpolateString("{{Data1 +\" \" + Data2}}", new Dictionary<string, object>()
            {
                {"Data1", "hello"},
                {"Data2", "world"}
            }));
            
            Assert.Equal("2",InterpolationHelper.InterpolateString("{{4 - Data1 - Data2 }}", new Dictionary<string, object>()
            {
                {"Data1", 1},
                {"Data2", 1}
            }));
            
            Assert.Equal("3",InterpolationHelper.InterpolateString("{{Data1.Count()}}", new Dictionary<string, object>()
            {
                {"Data1", new List<int>(){1,2,3}},
                {"Data2", 1}
            }));

            Assert.Equal("1",InterpolationHelper.InterpolateString("{{Data1[0]}}", new Dictionary<string, object>()
            {
                {"Data1", new List<int>(){1,2,3}},
                {"Data2", 1}
            }));
            //it refers to itself.
            Assert.Equal("6",InterpolationHelper.InterpolateString("{{Data1.Sum(it)}}", new Dictionary<string, object>()
            {
                {"Data1", new List<int>(){1,2,3}},
                {"Data2", 1}
            }));

            Assert.Equal("5", InterpolationHelper.InterpolateString("{{Data1.Sum(Item1)}}",
                new Dictionary<string, object>()
                {
                    {"Data1", new List<(int Value, int no)>() {(5, 0)}}
                }));
            Assert.Equal("10", InterpolationHelper.InterpolateString("{{Data1.Sum(X)}}",
                new Dictionary<string, object>()
                {
                    {
                        "Data1", new List<ComplexObject>()
                        {new ComplexObject()
                            {
                                X = 5
                            },new ComplexObject()
                            {
                                X = 5
                            }
                        }
                    }
                }));
        }

        class ComplexObject
        {
            public int X { get; set; }
        }
    }
}