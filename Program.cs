//using System;

//using System.Collections.Generic;
//using System.Windows.Forms;

////! @cond
//namespace UareUSampleCSharp
//{
//    static class Program
//    {
//        /// <summary>
//        /// The main entry point for the application.
//        /// </summary>
//        [MTAThread]
//        static void Main()
//        {
//            #if (!WindowsCE)
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);
//            Application.EnableVisualStyles();
//            #endif
//            Application.Run(new Form_Main());


//        }
//    }
//}
////! @endcond
///

//using System;
//using System.Collections.Generic;
//using System.Windows.Forms;

//namespace UareUSampleCSharp
//{
//    static class Program
//    {
//        /// <summary>
//        /// The main entry point for the application.
//        /// </summary>
//        [MTAThread]
//        static void Main(string[] args)   // 👈 accept arguments
//        {
//#if (!WindowsCE)
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);
//#endif

//            if (args.Length > 0)
//            {
//                string arg = args[0].ToLower();

//                if (arg.Contains("enroll"))
//                {
//                    Application.Run(new Enrollment(arg)); // 👈 pass argument
//                    return;
//                }
//                else if (arg.Contains("verify"))
//                {
//                    Application.Run(new Verification()); // 👈 optional, if you add verify later
//                    return;
//                }
//                else if (arg.Contains("attendance"))
//                {
//                    Application.Run(new Identification());
//                    return;
//                }
//            }

//            // Default behavior if no args provided
//            Application.Run(new Form_Main());
//        }
//    }
//}

//using System;
//using System.Windows.Forms;

//namespace UareUSampleCSharp
//{
//    static class Program
//    {
//        [MTAThread]
//        static void Main(string[] args)
//        {
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);

//            // Always create Form_Main (manages the reader)
//            var mainForm = new Form_Main();

//            // If launched with biometrics:// protocol
//            if (args.Length > 0 && args[0].StartsWith("biometrics://"))
//            {
//                string employeeId = null;

//                try
//                {
//                    var uri = new Uri(args[0]);

//                    if (!string.IsNullOrEmpty(uri.Query))
//                    {
//                        var queryParams = uri.Query.TrimStart('?').Split('&');
//                        foreach (var param in queryParams)
//                        {
//                            var parts = param.Split('=');
//                            if (parts.Length == 2 && parts[0] == "employee_id")
//                            {
//                                employeeId = Uri.UnescapeDataString(parts[1]);
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("Invalid biometrics URL: " + ex.Message, "Error",
//                        MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }

//                // Open Enrollment directly, passing employeeId + mainForm as _sender
//                var enrollForm = new Enrollment(employeeId)
//                {
//                    _sender = mainForm
//                };

//                Application.Run(enrollForm);
//            }
//            else
//            {
//                // Normal app startup
//                Application.Run(mainForm);
//            }
//        }
//    }
//}


//Third attempt

//using System;
//using System.Windows.Forms;

//namespace UareUSampleCSharp
//{
//    static class Program
//    {
//        [MTAThread]
//        static void Main(string[] args)
//        {
//            Application.EnableVisualStyles();
//            Application.SetCompatibleTextRenderingDefault(false);

//            // Always create Form_Main (manages the reader)
//            var mainForm = new Form_Main();

//            // Check if launched via biometrics:// protocol
//            if (args.Length > 0 && args[0].StartsWith("biometrics://"))
//            {
//                string employeeId = null;

//                try
//                {
//                    // Parse the URL
//                    var uri = new Uri(args[0]);

//                    if (!string.IsNullOrEmpty(uri.Query))
//                    {
//                        // uri.Query starts with "?", so we trim it
//                        var queryParams = uri.Query.TrimStart('?').Split('&');
//                        foreach (var param in queryParams)
//                        {
//                            var parts = param.Split('=');
//                            if (parts.Length == 2 && parts[0] == "employee_id")
//                            {
//                                employeeId = Uri.UnescapeDataString(parts[1]);
//                                break; // we found it, no need to continue
//                            }
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("Invalid biometrics URL: " + ex.Message, "Error",
//                        MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }

//                // Open Enrollment directly, passing employeeId + mainForm as _sender
//                var enrollForm = new Enrollment(employeeId)
//                {
//                    _sender = mainForm
//                };

//                Application.Run(enrollForm);
//            }
//            else
//            {
//                // Normal app startup
//                Application.Run(mainForm);
//            }
//        }
//    }
//}

//Fourth attempt

using System;
using System.Windows.Forms;

namespace UareUSampleCSharp
{
    static class Program
    {
        [MTAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Always create Form_Main (manages the reader)
            var mainForm = new Form_Main();

            if (args.Length > 0 && args[0].StartsWith("biometrics://"))
            {
                string employeeId = null;
                string action = null;

                try
                {
                    var uri = new Uri(args[0]);
                    action = uri.Host; // "enroll" or "identify"

                    if (!string.IsNullOrEmpty(uri.Query))
                    {
                        var queryParams = uri.Query.TrimStart('?').Split('&');
                        foreach (var param in queryParams)
                        {
                            var parts = param.Split('=');
                            if (parts.Length == 2 && parts[0] == "employee_id")
                            {
                                employeeId = Uri.UnescapeDataString(parts[1]);
                                break;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid biometrics URL: " + ex.Message, "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                // Decide which form to launch
                Form formToRun = null;
                if (action == "enroll")
                {
                    formToRun = new Enrollment(employeeId) { _sender = mainForm };
                }
                else if (action == "identify")
                {
                    formToRun = new Identification() { _sender = mainForm };
                }
                else if (action == "verify")
                {
                    formToRun = new Verification() { _sender = mainForm };
                }

                if (formToRun != null)
                    Application.Run(formToRun);
                else
                    Application.Run(mainForm);
            }
            else
            {
                // Normal app startup
                Application.Run(mainForm);
            }
        }
    }
}




