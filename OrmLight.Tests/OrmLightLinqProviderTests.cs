using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Moq;
using OrmLight.Enums;
using OrmLight.Linq;
using System.Linq;

namespace OrmLight.Tests
{
    [TestFixture]
    public class OrmLightLinqProviderTests
    {
        private static Mock<IDataAccessLayer> GetDAL()
        {
            var mock = new Mock<IDataAccessLayer>();
            mock.Setup(dal => dal.Get<User>()).Returns(new QueryableSource<User>(mock.Object, Operation.Read));
            return mock;
        }

        [Test]
        public void QueryInfo_IsNotNull_Test()
        {
            var queryInfo = GetDAL().Object.Get<User>();
            Assert.IsNotNull(queryInfo);    
        }

        [Test]
        public void QueryInfo_Type_Test()
        {
            var queryInfo = GetDAL().Object.Get<User>().GetQueryInfo();
            Assert.AreEqual(queryInfo.GetType(), typeof(QueryInfo));
        }

        [Test]
        public void QueryInfo_SingleConditionCount_Test()
        {
            var queryInfo = GetDAL().Object.Get<User>().AddCondition(u => u.Id == 10).GetQueryInfo();
            Assert.IsNotNull(queryInfo.Conditions);
            Assert.AreEqual(queryInfo.Conditions.Count, 1);            
        }

        [Test]
        public void QueryInfo_SingleCondition_Test()
        {
            var queryInfo = GetDAL().Object.Get<User>().AddCondition(u => u.Id == 10).GetQueryInfo();
            var condition = queryInfo.Conditions.First();
            Assert.AreEqual(condition.Operator, ConditionOperator.Equal);
            Assert.AreEqual(condition.LeftOperand, "Id");
            Assert.AreEqual(condition.RightOperand, 10);
        }

        private class User
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }    
        }
    }
}
