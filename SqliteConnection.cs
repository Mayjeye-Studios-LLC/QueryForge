using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEngine;

namespace QueryForge
{
    public class SqliteConnection:IDisposable
    { 
        
    [DllImport("sqlite3", EntryPoint = "sqlite3_open")]
    private static extern int sqlite3_open(string filename, out IntPtr db);

    [DllImport("sqlite3", EntryPoint = "sqlite3_close")]
    private static extern int sqlite3_close(IntPtr db);

    [DllImport("sqlite3", EntryPoint = "sqlite3_exec")]
    private static extern int sqlite3_exec(IntPtr db, string sql, IntPtr callback, IntPtr arg, out IntPtr errMsg);

    private IntPtr db; // Database pointer
    public bool IsNew { get; private set; }

    public SqliteConnection(string fileName)
    {
        var directory = Path.GetDirectoryName(fileName);
        if(!Directory.Exists(directory) && !string.IsNullOrWhiteSpace(directory))
            Directory.CreateDirectory(directory);
        IsNew = !File.Exists(fileName);
        OpenDatabase(fileName);

    }
   

    /// <summary>
    /// Opens a connection to the SQLite database.
    /// </summary>
    public void OpenDatabase(string path)
    {
        if (sqlite3_open(path, out db) != 0)
        {
            Debug.LogError($"Failed to open database: {path}");
        }
        else
        {
            Debug.Log($"Database opened successfully: {path}");
        }
    }

    /// <summary>
    /// Closes the database connection.
    /// </summary>
    public void CloseDatabase()
    {
        if (db != IntPtr.Zero)
        {
            if (sqlite3_close(db) == 0)
            {
                Debug.Log("Database closed successfully.");
                db = IntPtr.Zero;
            }
            else
            {
                Debug.LogError("Failed to close database.");
            }
        }
    }
    
    public string ExecuteSQL(string sql)
    {
        IntPtr errMsg = IntPtr.Zero;
        string resultData = ""; // To store the result

        // Callback function to process query results
        int Callback(IntPtr userData, int numCols, IntPtr colValues, IntPtr colNames)
        {
            // Ensure the result is not null
            if (numCols > 0)
            {
                IntPtr valuePtr = Marshal.ReadIntPtr(colValues); // Read the first column value
                resultData = Marshal.PtrToStringAnsi(valuePtr) ?? "NULL"; // Convert to string
            }
            return 0; // Return SQLite_OK
        }

        // Convert the callback function to a function pointer
        sqlite3_exec_callback callbackDelegate = Callback;
        IntPtr callbackPtr = Marshal.GetFunctionPointerForDelegate(callbackDelegate);

        // Execute the SQL command
        int result = sqlite3_exec(db, sql, callbackPtr, IntPtr.Zero, out errMsg);

        if (result != 0) // If SQLite_OK is not returned
        {
            string errorMessage = Marshal.PtrToStringAnsi(errMsg);
            Debug.LogError($@"There was an exception when runnning this sql query Message:{errorMessage}, SQL in question : '{sql}'");
            sqlite3_free(errMsg); // Free error message memory
        }
        else
        {
            Debug.Log($"SQL executed successfully: {sql}");
        }

        return resultData; // Return the result as a string
    }



// Delegate for sqlite3_exec callback
    private delegate int sqlite3_exec_callback(IntPtr userData, int numCols, IntPtr colValues, IntPtr colNames);

    // Free SQLite error message memory
    [DllImport("sqlite3", EntryPoint = "sqlite3_free")]
    private static extern void sqlite3_free(IntPtr errMsg);
    
        public void Dispose()
        {
            CloseDatabase();
        }
    }
}