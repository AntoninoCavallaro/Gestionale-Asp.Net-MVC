using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ClickClok.Helpers
{
    public static class SessionHelper
    {
        // Metodo per salvare un intero nella sessione
        public static void SetInt(ISession session, string key, int value)
        {
            session.SetInt32(key, value);
        }

        // Metodo per salvare una stringa nella sessione
        public static void SetString(ISession session, string key, string value)
        {
            session.SetString(key, value);
        }

        // Metodo per recuperare un intero dalla sessione
        public static int? GetInt(ISession session, string key)
        {
            return session.GetInt32(key);
        }

        // Metodo per recuperare una stringa dalla sessione
        public static string? GetString(ISession session, string key)
        {
            return session.GetString(key);
        }
    }

}
