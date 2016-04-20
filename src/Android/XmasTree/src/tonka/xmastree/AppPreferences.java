package tonka.xmastree;

import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.os.Bundle;
import android.preference.Preference;
import android.preference.PreferenceActivity;
import android.preference.PreferenceManager;

public class AppPreferences extends PreferenceActivity implements OnSharedPreferenceChangeListener {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        // TODO Auto-generated method stub
        super.onCreate(savedInstanceState);
        addPreferencesFromResource(R.xml.preferences);
        
        SharedPreferences sharedPreferences = getPreferenceManager().getSharedPreferences() ;
        Preference systemcAddress = findPreference("systemc_simulator_address");
        systemcAddress.setSummary(sharedPreferences.getString("systemc_simulator_address", ""));
    }
    
    public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key) {
        if (key.equals("systemc_simulator_address")) {
            Preference systemcAddress = findPreference(key);
            // Set summary to be the user-description for the selected value
            systemcAddress.setSummary(sharedPreferences.getString(key, ""));
        }
    }
    
    @Override
    protected void onResume() {
        super.onResume();
        getPreferenceScreen().getSharedPreferences()
                .registerOnSharedPreferenceChangeListener(this);
    }

    @Override
    protected void onPause() {
        super.onPause();
        getPreferenceScreen().getSharedPreferences()
                .unregisterOnSharedPreferenceChangeListener(this);
    }
}