namespace NickUtilsTest;

using static NicUtils.TestHelpers;
using NicUtils;

[TestClass]
public class FileReadersTests {
    [TestMethod]
    public void ReadNullableFloatDataWithoutHeaders() {
        CSVReader reader = new("../../../Resources/floatDataWithoutHeaders.csv", false);
        List<List<double?>> data = reader.GetData<double?>();
        List<string> headers = reader.Headers;
        
        int expectedRows = 3;
        Assert.AreEqual(expectedRows, data.Count);
        Assert.IsNull(headers);
        AssertSequencesAreEqual(data.ElementAt(0), ListOf<double?>(3.45, 6.4, 4.63));
        AssertSequencesAreEqual(data.ElementAt(1), ListOf<double?>(356.66, 33, 486.653));
        AssertSequencesAreEqual(data.ElementAt(2), ListOf<double?>(null, 0, 1));
    }

    [TestMethod]
    public void ReadNotNullableFloatDataWithoutHeaders() {
        CSVReader reader = new("../../../Resources/floatDataWithoutHeaders.csv", false);
        List<List<double>> data = reader.GetData<double>();
        List<string> headers = reader.Headers;

        int expectedRows = 3;
        Assert.AreEqual(expectedRows, data.Count);
        Assert.IsNull(headers);
        AssertSequencesAreEqual(data.ElementAt(0), ListOf(3.45, 6.4, 4.63));
        AssertSequencesAreEqual(data.ElementAt(1), ListOf(356.66, 33, 486.653));
        AssertSequencesAreEqual(data.ElementAt(2), ListOf<double>(0, 0, 1));
    }

    [TestMethod]
    public void ReadNullableFloatDataWithHeaders() {
        CSVReader reader = new("../../../Resources/floatDataWithHeaders.csv", true);
        List<List<double?>> data = reader.GetData<double?>();
        List<string> headers = reader.Headers;

        int expectedRows = 3;
        Assert.AreEqual(expectedRows, data.Count);
        AssertSequencesAreEqual(headers, new List<string>() { "col1", "col2", "col3" });
        AssertSequencesAreEqual(data.ElementAt(0), ListOf<double?>(3.45, 6.4, 4.63));
        AssertSequencesAreEqual(data.ElementAt(1), ListOf<double?>(356.66, 33, 486.653));
        AssertSequencesAreEqual(data.ElementAt(2), ListOf<double?>(null, 0, 1));
    }

    [TestMethod]
    public void ReadTextDataWithHeaders() {
        CSVReader reader = new("../../../Resources/textDataWithHeaders.csv", true);
        List<List<string>> data = reader.GetData<string>();
        List<string> headers = reader.Headers;

        int expectedRows = 2;
        Assert.AreEqual(expectedRows, data.Count);
        AssertSequencesAreEqual(headers, ListOf("col1", "col2", "col3" ));
        AssertSequencesAreEqual(data.ElementAt(0), ListOf("hello", null, "col 2 is empty!"));
        AssertSequencesAreEqual(data.ElementAt(1), ListOf("goodbye", "this col 2 is not empty", "2.678"));
    }
}