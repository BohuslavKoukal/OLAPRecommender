using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Recommender.Business.FileHandling;
using Recommender.Business.FileHandling.Csv;
using Recommender.Common;
using RecommenderTests.Helpers;

namespace RecommenderTests.BusinessLayerTests
{
    [TestClass]
    public class FileHandlingTests
    {
        private Mock<IConfiguration> _configuration;
        private CsvHandler _csvHandler;
        private readonly IFileHandler _testee;
        private readonly Mock<HttpPostedFileBase> _uploadedFile;
        private FileStream _stream;

        public FileHandlingTests()
        {
            _configuration = new Mock<IConfiguration>();
            _csvHandler = new CsvHandler();
            _testee = new FileHandler(_configuration.Object, _csvHandler);
            _uploadedFile = new Mock<HttpPostedFileBase>();
        }

        private void SetupCsvFile()
        {
            _uploadedFile.Reset();
            _configuration.Reset();
            _stream = new FileStream($"{BusinessLayerTestHelper.DataLocation}\\testData.csv", FileMode.Open);
            _uploadedFile.Setup(x => x.InputStream).Returns(_stream);
            _uploadedFile.Setup(x => x.ContentLength).Returns((int)_stream.Length);
            _uploadedFile.Setup(x => x.FileName).Returns(_stream.Name);
            _configuration.Setup(c => c.GetFilesLocation()).Returns($"{BusinessLayerTestHelper.DataLocation}\\");
        }

        private void SetupRdfFile()
        {
            _uploadedFile.Reset();
            _configuration.Reset();
            _stream = new FileStream($"{BusinessLayerTestHelper.DataLocation}\\usti.ttl", FileMode.Open);
            _uploadedFile.Setup(x => x.InputStream).Returns(_stream);
            _uploadedFile.Setup(x => x.ContentLength).Returns((int)_stream.Length);
            _uploadedFile.Setup(x => x.FileName).Returns(_stream.Name);
            _configuration.Setup(c => c.GetFilesLocation()).Returns($"{BusinessLayerTestHelper.DataLocation}\\");
        }

        [TestMethod]
        public void TestSaveCsvFile()
        {
            // Arrange
            SetupCsvFile();
            // Act
            var file = _testee.SaveFile(_uploadedFile.Object, ";");
            // Assert
            Path.GetDirectoryName(file.FilePath).ShouldBeEquivalentTo($"{ BusinessLayerTestHelper.DataLocation}");
            file.Attributes.Count.ShouldBeEquivalentTo(5);
            file.Attributes.ToList()[0].Name.ShouldBeEquivalentTo("Place");
            file.Attributes.ToList()[1].Name.ShouldBeEquivalentTo("Region");
            file.Attributes.ToList()[2].Name.ShouldBeEquivalentTo("Product");
            file.Attributes.ToList()[3].Name.ShouldBeEquivalentTo("Category");
            file.Attributes.ToList()[4].Name.ShouldBeEquivalentTo("Units");
        }

        [TestMethod]
        public void TestGetValues()
        {
            // Arrange
            SetupCsvFile();
            // Act
            var values = _testee.GetValues(_uploadedFile.Object.FileName, BusinessLayerTestHelper.GetDimensionOrMeasureDtos(), ";", "dd.MM.yyyy");
            // Assert
            values.Rows.Count.ShouldBeEquivalentTo(9);
            values.Columns.Count.ShouldBeEquivalentTo(5);
            values.Rows[0]["Place"].ShouldBeEquivalentTo("Czech_republic");
            values.Rows[0]["Region"].ShouldBeEquivalentTo("Europe");
            values.Rows[0]["Product"].ShouldBeEquivalentTo("Bread");
            values.Rows[0]["Category"].ShouldBeEquivalentTo("Bakery");
            values.Rows[0]["Units"].ShouldBeEquivalentTo("20");

            values.Rows[8]["Place"].ShouldBeEquivalentTo("Russia");
            values.Rows[8]["Region"].ShouldBeEquivalentTo("Asia");
            values.Rows[8]["Product"].ShouldBeEquivalentTo("Milk");
            values.Rows[8]["Category"].ShouldBeEquivalentTo("Dairy");
            values.Rows[8]["Units"].ShouldBeEquivalentTo("35");
        }

        [TestMethod]
        public void TestSaveRdfFile()
        {
            // Arrange
            SetupRdfFile();
            // Act
            var file = _testee.SaveFile(_uploadedFile.Object, null);
            // Assert
            Path.GetDirectoryName(file.FilePath).ShouldBeEquivalentTo($"{ BusinessLayerTestHelper.DataLocation}");
            file.Attributes.ShouldBeEquivalentTo(null);
        }



    }
}
