function decodeUplink(input) {
    var bytes = input.bytes
    return {
      data: {
        battery : (bytes[0]<<8 | bytes[1]),
        capacitance1 : ((bytes[2] & 0x80 ? 0xFFFF<<16 : 0) | bytes[2]<<8 | bytes[3]),
        tempc1 : ((bytes[4] & 0x80 ? 0xFFFF<<16 : 0) | bytes[4]<<8 | bytes[5]) / 10.0,
        capacitance2 : ((bytes[6] & 0x80 ? 0xFFFF<<16 : 0) | bytes[6]<<8 | bytes[7]),
        capacitance3 : ((bytes[8] & 0x80 ? 0xFFFF<<16 : 0) | bytes[8]<<8 | bytes[9]),
      },
      warnings: [],
      errors: []
    };
  }
