using System;
using System.ComponentModel;
using System.Reflection;

namespace RMRS.ConsoleApp.Helpers
{
    public static class CommonHelper
    {
        /// <summary>
        /// Проверка на повторный запуск приложения
        /// </summary>
        /// <param name="instanceMutex"></param>
        /// <param name="applicationUniqueName">Уникальный id, например, некий hard-coded Guid</param>
        /// <returns>Is started already</returns>
        public static bool GetInstanceCheck(ref Mutex? instanceMutex, string applicationUniqueName)
        {
            bool isNew;
            var mutex = new Mutex(true, applicationUniqueName, out isNew);
            if (isNew)
            {
                instanceMutex = mutex;
            }
            else
            {
                mutex.Dispose(); // Drop immediately
            }

            return !isNew;
        }
    }
}