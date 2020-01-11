function Decoder(b, port) {

    var d = {};
      d.battery = (b[0]<<8 | b[1]);
      d.capacitance1 = ((b[2] & 0x80 ? 0xFFFF<<16 : 0) | b[2]<<8 | b[3]);
      d.tempc1 = ((b[4] & 0x80 ? 0xFFFF<<16 : 0) | b[4]<<8 | b[5]) / 10.0;
      d.capacitance2 = ((b[6] & 0x80 ? 0xFFFF<<16 : 0) | b[6]<<8 | b[7]);
      d.capacitance3 = ((b[8] & 0x80 ? 0xFFFF<<16 : 0) | b[8]<<8 | b[9]);
      return d;
  }