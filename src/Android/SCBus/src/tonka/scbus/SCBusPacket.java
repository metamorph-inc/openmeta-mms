package tonka.scbus;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;
import java.util.Arrays;

/*
 * SCBus Packet Format
 * 
 * -----------------------------------------------------------
 * | MAGIC(4) | TIME STAMP(8) | CID(4) | LENGTH(4) | DATA ...|
 * -----------------------------------------------------------
 * 
 * Magic is a well known 4 byte sequence: 'A2SC' from Android to Systemc and 'SC2A' on the other direction
 * Time stamps (64bit signed) are in microseconds (local/sender time from start of simulation)
 * CID is the channel ID (32bit signed), special channel ID (0) is used for time sync/heart beat
 * Data is of variable  size, specified by the Length field (32bit signed) 
 */
public class SCBusPacket {
	public enum Direction {
		AndroidToSystemC, SystemCToAndroid
	}
	public final static int MAX_DATA_LENGTH = 512;
			
	public final static int MAGIC_OFFSET = 0;
	public final static int MAGIC_LENGTH = 4; 
	
	public final static int TIMESTAMP_OFFSET = MAGIC_OFFSET + MAGIC_LENGTH;
	public final static int TIMESTAMP_LENGTH = 8;
	
	public final static int CID_OFFSET = TIMESTAMP_OFFSET + TIMESTAMP_LENGTH;
	public final static int CID_LENGTH = 4;
	
	public final static int LENGTH_OFFSET = CID_OFFSET + CID_LENGTH;
	public final static int LENGTH_LENGTH = 4;
	
	public final static int DATA_OFFSET = LENGTH_OFFSET + LENGTH_LENGTH;
	
	public final static int HEADER_LENGTH = DATA_OFFSET;
	public final static int MAX_PACKET_LENGTH = MAX_DATA_LENGTH + HEADER_LENGTH;
	
	public final static byte[] MAGIC_A2SC = "A2SC".getBytes();
	public final static byte[] MAGIC_SC2A = "SC2A".getBytes();
	
	private long timeStamp;
	private int cid;
	private byte[] data;
	private Direction dir;
	
	public SCBusPacket(Direction dir, long timeStamp, int cid, byte[] data) {
		this.dir = dir;
		this.timeStamp = timeStamp;
		this.cid = cid;
		this.data = data;
	}

	public long getTimeStamp() {
		return timeStamp;
	}

	public int getCid() {
		return cid;
	}

	public byte[] getData() {
		return data;
	}

	public Direction getDir() {
		return dir;
	}
	
	static public SCBusPacket deserialize(byte[] raw)
	{
		ByteBuffer buf = ByteBuffer.wrap(raw);
		buf.order(ByteOrder.LITTLE_ENDIAN);

		byte[] magic = new byte[SCBusPacket.MAGIC_SC2A.length];
		buf.get(magic);

		if (!Arrays.equals(magic, SCBusPacket.MAGIC_SC2A)) {
			return null;
		}
		
		long timestamp = buf.getLong();
		int cid = buf.getInt();
		int length = buf.getInt();
		byte[] data = new byte[length];
		buf.get(data);

		return new SCBusPacket(Direction.SystemCToAndroid, timestamp, cid, data);
	}

	// Serialize
	byte[] serialize() {
		int size = HEADER_LENGTH + (data == null ? 0 : data.length); 
		ByteBuffer buf = ByteBuffer.allocate(size);
		buf.order(ByteOrder.LITTLE_ENDIAN);
		
		buf.put(dir == Direction.AndroidToSystemC ? MAGIC_A2SC : MAGIC_SC2A);
		buf.putLong(timeStamp);
		buf.putInt(cid);
		if (data != null) {
			buf.putInt(data.length);
			buf.put(data);
		}
		else {
			buf.putInt(0);
		}
		
		buf.flip();
		byte[] raw = new byte[size];
		buf.get(raw);
		
		return raw;
	}
	
	public String toString() {
		return (dir == Direction.AndroidToSystemC ? "A->SC" : "SC->A") +
				"@" + timeStamp/1000 + "us" +
				", length=" + (data!=null ? data.length : 0);
	}
}
