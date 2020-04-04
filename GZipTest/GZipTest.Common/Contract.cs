using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace GZipTest.Common
{
    public class Contract
    {
        #region Methods and other members

        public static void IsInstanceOfType<T>(object target) => Requires(target is T);

        public static void IsNotNull(object target, [CallerLineNumber] int sourceLineNumber = 0, [CallerFilePath] string sourceFilePath = "") => Requires<ApplicationException>(target != null, () => "Provided object is null", sourceLineNumber, sourceFilePath);

        public static void Requires(bool result, [CallerLineNumber] int sourceLineNumber = 0, [CallerFilePath] string sourceFilePath = "") => Requires<ApplicationException>(result, null, sourceLineNumber, sourceFilePath);

        public static void Requires<T>(bool result, Func<string> message, [CallerLineNumber] int sourceLineNumber = 0, [CallerFilePath] string sourceFilePath = "") where T : Exception
        {
            if (!result)
            {
                string msg = message == null ? "Contract.Requires failed." : $"{message()}.";
                if (!typeof(UserException).IsAssignableFrom(typeof(T)))
                {
                    msg = $"{msg} {sourceFilePath} {sourceLineNumber}";
                }

                var ex = (T) Activator.CreateInstance(typeof(T));
                typeof(T).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(ex, msg);
                throw ex;
            }
        }

        #endregion
    }
}