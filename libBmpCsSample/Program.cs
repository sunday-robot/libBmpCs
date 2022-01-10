using libBmpCs;

var (data, width, height) = Bmp.Load("../../../sample.bmp");
Bmp.Save("../../../sample2.bmp", data, width, height);
