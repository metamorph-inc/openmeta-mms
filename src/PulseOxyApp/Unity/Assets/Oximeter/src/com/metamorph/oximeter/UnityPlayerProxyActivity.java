package com.metamorph.oximeter;

import com.unity3d.player.*;
import java.util.Map;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Build;
import android.os.Bundle;

public class UnityPlayerProxyActivity extends Activity
{
	protected void onCreate (Bundle savedInstanceState)
	{
		super.onCreate(savedInstanceState);

		// If the (Native)Activity is overridden these class names must match the new activities.
		String classNames[] = { "com.metamorph.oximeter.UnityPlayerActivity", "com.metamorph.oximeter.UnityPlayerNativeActivity" };

		// Convert old PlayerPrefs (pre Unity 3.4) to new PlayerPrefs
		copyPlayerPrefs(this, classNames);

		// Start the most 'advanced' Activity supported by the current Android OS.
		// (Android OS 2.3 ('Gingerbread') and above supports NativeActivity)
		try
		{
			boolean supportsNative = Build.VERSION.SDK_INT >= 9 /*Build.VERSION_CODES.GINGERBREAD*/;
			Class<?> activity = Class.forName(classNames[supportsNative ? 1 : 0]);
			Intent intent = new Intent(this, activity);
			intent.addFlags(Intent.FLAG_ACTIVITY_NO_ANIMATION);

			Bundle extras = getIntent().getExtras();
			if (extras != null)
				intent.putExtras(extras);
	
			startActivity(intent);
		}
		catch (ClassNotFoundException e)
		{
			e.printStackTrace();
		}
		finally
		{
			finish();
		}
	}

	static protected void copyPlayerPrefs(Context context, String[] activityClassNames)
	{
		// UnityPlayer uses PackageName (bundle identifier) as PlayerPrefs identifier, starting from Unity 3.4.
		SharedPreferences packagePrefs = context.getSharedPreferences(context.getPackageName(), Context.MODE_PRIVATE);

		// If PlayerPrefs<package_name> already exists there is no need to
		// copy the old values; they might in fact be stale data.
		if (!packagePrefs.getAll().isEmpty())
			return;

		// Loop through the Activities and copy the contents (if any) of associated PlayerPrefs (Unity 3.3 and earlier).
		SharedPreferences.Editor playerPrefs = packagePrefs.edit();
		for (String name : activityClassNames)
		{
			SharedPreferences prefs = context.getSharedPreferences(name, Context.MODE_PRIVATE);
			java.util.Map<String,?> keys = prefs.getAll();
			if (keys.isEmpty())
				continue;
			for (Map.Entry<String, ?> entry : keys.entrySet())
			{
				Object value = entry.getValue();
				if (value.getClass() == Integer.class)
					playerPrefs.putInt(entry.getKey(), (Integer)value);
				else if (value.getClass() == Float.class)
					playerPrefs.putFloat(entry.getKey(), (Float)value);
				else if (value.getClass() == String.class)
					playerPrefs.putString(entry.getKey(), (String)value);
			}
			playerPrefs.commit();
		}
	}
}
