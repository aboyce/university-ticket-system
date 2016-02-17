package ts.ticketmanagement;

import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.AsyncTask;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.Objects;

public class MainActivity extends AppCompatActivity {

    static final int LOGIN_FROM_MAIN = 0;

    private String username;
    private String userToken;

    private boolean credentialsConfirmed = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Log.d("TICKET_MANAGEMENT", "'ACTIVITY_NAME':'METHOD_NAME':'INFORMATION'");
        Log.d("TICKET_MANAGEMENT", "MainActivity:onCreate");

        setContentView(R.layout.activity_main);

        // If the phone cannot find a valid username/userToken, then assume they are
        // not 'logged in', so send them to the Login Page.
        if(!userConfiguredWithApplication()){
            Log.d("TICKET_MANAGEMENT", "MainActivity:onCreate: User not configured with app.");
            Intent loginIntent = new Intent(this, LoginActivity.class);
            startActivityForResult(loginIntent, LOGIN_FROM_MAIN);
        }

        // We now assume that the phone has valid credentials, so we can check they are valid.
        Log.d("TICKET_MANAGEMENT", "MainActivity:onCreate: User is configured with app.");

        new API_ConfirmUserCredentials().execute();
    }

    @Override
    protected void onActivityResult(int pRequestCode, int pResultCode, Intent pData){
        Log.d("TICKET_MANAGEMENT", "MainActivity:onActivityResult");
        if(pRequestCode == LOGIN_FROM_MAIN){
            if (pResultCode == RESULT_OK){
                if(pData.hasExtra(getString(R.string.user_username)))
                    username = pData.getStringExtra(getString(R.string.user_username));
                if(pData.hasExtra(getString(R.string.user_token)))
                    userToken = pData.getStringExtra(getString(R.string.user_token));

                // TODO: remove this eventually
                showMessageBox("User is logged in! [RESULT_OK]", "Whooo, the saved information is, Username: " + username + " User Token: " + userToken);

                // TODO: Pop up asking if it is ok to confirm the authentication via SMS
//                Intent intent = new Intent(Intent.ACTION_SENDTO, Uri.parse("smsto:555555"));
//                intent.putExtra("sms_body", "The message body");
//                startActivity(intent);

                // TODO: Get back from sms....

                // TODO: Load the tickets via an api call...

                // TODO: This may be better as a MainActivity, and then load the tickets activity??


            }
            else if(pResultCode == RESULT_CANCELED){

                // TODO: remove this eventually
                showMessageBox("User not logged in [RESULT_CANCELED]", "ah well...");

                // TODO: maybe a sensible popup box
            }
        }
    }

    private boolean userConfiguredWithApplication()    {
        Log.d("TICKET_MANAGEMENT","MainActivity:userConfiguredWithApplication");
        SharedPreferences sharedPreferences = this.getSharedPreferences(getString(R.string.persistent_storage_name), Context.MODE_PRIVATE);

        if(sharedPreferences.contains(getString(R.string.persistent_storage_user_username)))
            username = sharedPreferences.getString(getString(R.string.persistent_storage_user_username), null);
        else
            return false;

        Log.d("TICKET_MANAGEMENT","MainActivity:userConfiguredWithApplication: Contained username.");

        if(sharedPreferences.contains(getString(R.string.persistent_storage_user_token)))
            userToken = sharedPreferences.getString(getString(R.string.persistent_storage_user_token), null);
        else
            return false;

        Log.d("TICKET_MANAGEMENT","MainActivity:userConfiguredWithApplication: Contained userToken.");

        return true;
    }

    private boolean userCredentialsValid()    {
        Log.d("TICKET_MANAGEMENT","MainActivity:userCredentialsValid");





        return true;
    }

    private void showMessageBox(String title, String message){
        Log.d("TICKET_MANAGEMENT","MainActivity:showMessageBox: Title='" + title + "' Message= " + message);
        AlertDialog.Builder messageBox = new AlertDialog.Builder(this);
        messageBox.setTitle(title);
        messageBox.setMessage(message);
        messageBox.setPositiveButton("OK", new DialogInterface.OnClickListener() {
            public void onClick(DialogInterface dialogInterface, int which) {
            }
        });
        messageBox.setCancelable(true);
        messageBox.create().show();
    }

    class API_ConfirmUserCredentials extends AsyncTask<Void, Void, String> {

        protected void onPreExecute(){
            Log.d("TICKET_MANAGEMENT", "LoginActivity-API_ConfirmUserCredentials");
            //progressbar.setVisibility(View.VISIBLE);
        }

        protected String doInBackground(Void... urls) {
            Log.d("TICKET_MANAGEMENT", "LoginActivity-API_ConfirmUserCredentials:doInBackground");
            try{
                URL url = new URL(getString(R.string.api_url) + getString(R.string.api_user_confirmUserToken1) + username + getString(R.string.api_user_confirmUserToken2) + userToken);
                HttpURLConnection urlConnection = (HttpURLConnection) url.openConnection();
                Log.d("TICKET_MANAGEMENT", "LoginActivity-API_ConfirmUserCredentials: Opened HTTP URL Connection to; " + url.toString());
                try{
                    BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(urlConnection.getInputStream()));
                    StringBuilder stringBuilder = new StringBuilder();
                    String line;
                    while((line = bufferedReader.readLine()) != null)
                        stringBuilder.append(line).append("\n");
                    bufferedReader.close();
                    Log.d("TICKET_MANAGEMENT", "LoginActivity-API_ConfirmUserCredentials:doInBackground: Response from API=" + stringBuilder.toString());
                    return stringBuilder.toString();
                }catch (Exception e){
                    Log.e("TICKET_MANAGEMENT", "LoginActivity-API_ConfirmUserCredentials:doInBackground: Error: " + e.getMessage(), e);
                    return null;
                }
                finally {
                    urlConnection.disconnect();
                }
            }catch (Exception e){
                Log.e("TICKET_MANAGEMENT", "LoginActivity-API_ConfirmUserCredentialsn:doInBackground: Error: " + e.getMessage(), e);
                return null;
            }
        }

        protected void onPostExecute(String response){
            Log.d("TICKET_MANAGEMENT", "LoginActivity-API_ConfirmUserCredentials:onPostExecute");
            if(response == null){
                showMessageBox("Error Confirming Credentials", "An error has occurred trying to confirm your user token, please check the configuration and try again");
                Log.e("TICKET_MANAGEMENT","LoginActivity-API_ConfirmUserCredentials:onPostExecute: Error: " + response);
                return;
            }

            response = response.replace("\"","");

            if(response == "true")
                credentialsConfirmed = true;

            //progressbar.setVisibility(View.GONE);
        }
    }
}
