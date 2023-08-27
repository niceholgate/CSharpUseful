namespace NickUtilsTest;

using static NicUtils.TestHelpers;
using NicUtils;

[TestClass]
public class FileReadersTests {
    [TestMethod]
    public void ReadFloatDataWithoutHeaders() {
        List<List<string>> data = CSVReader.ReadCSV("../../../Resources/floatDataWithoutHeaders.csv");
        int expectedRows = 3;
        Assert.AreEqual(expectedRows, data.Count);
        AssertSequencesAreEqual(data.ElementAt(0), new List<string>(){ "3.45", "6.4", "4.63" });
        AssertSequencesAreEqual(data.ElementAt(1), new List<string>() { "356.66", "33", "486.653" });
        AssertSequencesAreEqual(data.ElementAt(2), new List<string>() { null, "0", "1" });
    }
}