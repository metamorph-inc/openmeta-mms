import processing.net.*;
import java.nio.ByteBuffer;
import java.nio.ByteOrder;

int port = 7674;
Server scServer;

void setup()
{
  size(400, 400);
  textFont(createFont("SanSerif", 16));
  scServer = new Server(this, port); // Starts a myServer on port 10002
  background(0);
}

void mousePressed()
{
  scServer.stop();
  exit();
}

void draw()
{
  Client c = scServer.available();
  if (c !=null) {
    byte[] msg = c.readBytes();
    ByteBuffer buffer = ByteBuffer.allocate(msg.length);
    buffer.order(ByteOrder.LITTLE_ENDIAN);
    buffer.put(msg);
    buffer.rewind();
    byte[] magic = new byte[4];
    buffer.get(magic);
    long timeStamp = buffer.getLong();
    int cid = buffer.getInt();
    int length = buffer.getInt();
    byte[] data = new byte[length];
    buffer.get(data);
    if (msg != null) {
      println(String.format("%s: %s @%d [%d] %s", c.ip(), new String(magic), timeStamp, cid, new String(data)));
    }
  }
}

void serverEvent(Server s, Client c) {
  println("Client connected :" + c.ip());
}

void keyPressed() {
  if (key == 's' || key == 'S') {
    byte[] data = "juhe".getBytes();
    ByteBuffer buffer = ByteBuffer.allocate(data.length + 20);
    buffer.order(ByteOrder.LITTLE_ENDIAN);
    buffer.put("SC2A".getBytes());
    buffer.putLong(System.nanoTime());
    buffer.putInt(1);
    buffer.putInt(data.length);
    buffer.put(data);
    scServer.write(buffer.array());
  }
}

