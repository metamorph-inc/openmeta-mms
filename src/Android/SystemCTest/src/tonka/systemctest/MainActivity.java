package tonka.systemctest;

import tonka.scbus.SCBus;
import tonka.scbus.SCBusListener;
import android.app.Activity;
import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.util.Log;
import android.view.Menu;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ToggleButton;

public class MainActivity extends Activity implements SCBusListener {

	private final String TAG = "SystemCTest";
	private SCBus bus;
	
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        bus = new SCBus();
        EditText text = (EditText) findViewById(R.id.receivedText);
        text.setMovementMethod(new ScrollingMovementMethod());
    }

    public void onBusButton(View view) {
    	boolean on = ((ToggleButton) view).isChecked();
        if (on) {
        	bus.open(this);
        	
        } else {
        	bus.close();
        }
        Button sendButton = (Button) findViewById(R.id.sendButton);
        sendButton.setEnabled(on);
    }
    
    public void onSendButton(View view) {
    	EditText text = (EditText) findViewById(R.id.sendText);
    	bus.send(text.getText().toString().getBytes());
    	text.setText("");
    }
    
    protected void onDestroy() {
    	Log.d(TAG, "onDestroy");
    	bus.close();
    	super.onDestroy();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

	@Override
	public void dataReceived(byte[] data) {
		Log.d(TAG, "dataReceived :" + new String(data));
		EditText text = (EditText) findViewById(R.id.receivedText);
		text.append(new String(data));
	}
    
    
}
