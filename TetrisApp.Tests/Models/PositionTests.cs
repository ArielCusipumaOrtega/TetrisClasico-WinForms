using TetrisApp.Models;
using Xunit;

namespace TetrisApp.Tests.Models
{
    public class PositionTests
    {
        [Fact]
        public void Constructor_SetsRowAndColumnCorrectly()
        {
            var pos = new Position(5, 8);

            Assert.Equal(5, pos.Row);
            Assert.Equal(8, pos.Column);
        }

        [Fact]
        public void Down_IncreasesRowByOne()
        {
            var pos = new Position(3, 4);
            var next = pos.Down();

            Assert.Equal(4, next.Row);
            Assert.Equal(4, next.Column);
        }

        [Fact]
        public void Up_DecreasesRowByOne()
        {
            var pos = new Position(3, 4);
            var next = pos.Up();

            Assert.Equal(2, next.Row);
            Assert.Equal(4, next.Column);
        }

        [Fact]
        public void Left_DecreasesColumnByOne()
        {
            var pos = new Position(3, 4);
            var next = pos.Left();

            Assert.Equal(3, next.Row);
            Assert.Equal(3, next.Column);
        }

        [Fact]
        public void Right_IncreasesColumnByOne()
        {
            var pos = new Position(3, 4);
            var next = pos.Right();

            Assert.Equal(3, next.Row);
            Assert.Equal(5, next.Column);
        }

        [Fact]
        public void Equality_EqualCoordinates_AreEqual()
        {
            var pos1 = new Position(2, 7);
            var pos2 = new Position(2, 7);
            var pos3 = new Position(2, 8);

            Assert.Equal(pos1, pos2);
            Assert.True(pos1 == pos2);
            Assert.False(pos1 == pos3);
            Assert.Equal(pos1.GetHashCode(), pos2.GetHashCode());
        }

        [Fact]
        public void ChainedMovements_ProduceExpectedPosition()
        {
            var initial = new Position(5, 5);
            var result = initial.Down().Down().Right().Left().Up();

            Assert.Equal(new Position(6, 5), result);
        }
    }
}
