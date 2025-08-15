struct UnknownData {
  byte[] data;

  public bool firstBitIsSet {
    get => data[0] & (1 << 7) != 0;
  }

  public bool isMaxValue {
    get => data.All(byte => byte == 0xFF);
  }

  public int GetPrefixLength() {
    // TODO
  }
}

public class NumericDistribution {
  public List<UnknownData> dataSet;
  public int fixedSize;
  public bool looksFloaty = false;
  public bool looksSigned = false;

  public NumericDistribution(int fixedSize, List<UnknownData> dataSet) {
    this.dataSet = dataSet;
    this.fixedSize = fixedSize;
    EvaluateDistribution();
  }

  private void EvaluateDistribution() {
    int maybeNegativeCount = 0;
    foreach (var entry in dataSet) {
      if (entry.firstBitIsSet && !entry.isMaxValue) maybeNegativeCount++;
    }
    var tenPercent = dataSet.Count / 10;
    // TODO: looks floaty?
    looksSigned = maybeNegativeCount > tenPercent;
  }

  public DataType {
    get {
      return fixedSize switch {
        4 => looksFloaty ? new FloatDataType() : 
            (looksSigned ? new Int32DataType() : new UInt32DataType()),
        2 => looksSigned ? new Int16DataType() : new UInt16DataType(),
        1 => looksSigned ? new Int8DataType() : new UInt8DataType(),
        _ => null
      }
    }
  }
}

public class StringDistribution {
  public List<UnknownData> dataSet;
  public int fixedSize;
  public int prefix = 0;
  public bool inconsistentPrefix;
  public bool looksStringy = false;

  public DataType {
    get => looksStringy ? new StringDataType(fixedSize, prefixLength) : null;
  }

  public StringDistribution(int fixedSize, List<UnknownData> dataSet) {
    this.dataSet = dataSet;
    this.fixedSize = fixedSize;
    EvaluateDistribution();
  }

  private void EvaluatePrefix(UnknownData entry) {
    if (inconsistentPrefix || fixedSize > 0) return;
    var prefix = entry.GetPrefixLength();
    if (prefix == 0) return;
    if (this.prefix > 0 && this.prefix != prefix) {
      incosistentPrefix = true;
    } else {
      this.prefix = prefix;
    }
  }

  private void EvaluateDistribution() {
    int totalCharacterCount = 0;
    int weirdCharacterCount = 0;
    foreach (var entry in dataSet) {
      EvaluatePrefix(entry);
      totalCharacterCount += entry.data.length - prefix;
      for (int i = 0; i < totalCharacterCount; i++) {
        var char = entry.data[prefix + i];
        if (char > 0 && char < 32 || char > 126) 
          weirdCharacterCount++;
      }
    }
    var onePercent = totalCharacterCount / 100;
    looksStringy = weirdCharacterCount < onePercent;
  }
}


public class DataHeuristics {
  public List<UnknownData> dataSet;
  public OrderedDictionary<int, int> lengths = new OrderedDictionary<int, int>();

  public DataHeuristics(List<UnknownData> dataSet) {
    this.dataSet = dataSet;
    EvaluateData();
  }

  private void EvaluateData() {
    foreach (var entry in dataSet) {
      var key = entry.data.length;
      if (!lengths.ContainsKey(key))
        lengths[key] = 0;
      lengths[key] += 1;
    }
  }

  public DataType GuessDataType() {
    return numericDistribution.DataType ?? 
      stringDistribution.DataType ?? 
      UnknownDataType;
  }
}

