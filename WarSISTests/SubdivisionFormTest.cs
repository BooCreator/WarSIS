using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;

using WarSIS.MainForms;

using WarSISModelsDB;
using WarSISModelsDB.Models;
using WarSISModelsDB.Models.Data;
using WarSISModelsDB.Models.DataBase;
using WarSISModelsDB.Models.DataBase.Subdivision;

namespace WarSISTests
{
    [TestClass]
    public class SubdivisionFormTest
    {
        [TestMethod]
        public void LoadTest()
        {
        }
        [TestMethod]
        public void AddTest()
        {
        }
        [TestMethod]
        public void GetTest()
        {
            var Form = new SubdivisionsForm(TestData.DataBase);
            Assert.IsTrue(Form.GetBuildings().Count > 0);
            Assert.IsTrue(Form.GetPeoplesInRanks().Count > 0);
            Assert.IsTrue(Form.GetPeoperties().Count > 0);

            Form.ActiveSubdivision = new Subdivisions() { Editor = TestData.DataBase }.GetSubdivision("Роты");
            Assert.IsTrue(Form.GetValidUpper().Count > 0);

            Form.ActiveItem = (Form.ActiveSubdivision as IDataBaseElement<ISubdivision>).Select()[0];
            Assert.IsTrue(Form.GetSubdivisionPeoperties().Count > 0);
        }
        [TestMethod]
        public void PrintTest()
        {
        }
        [TestMethod]
        public void SaveTest()
        {
        }
        [TestMethod]
        public void RemoveTest()
        {
        }
    }
}
