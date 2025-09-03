using System.Threading.Tasks;
using SerenityAITranslator.Editor.Session.Models;
using SerenityAITranslator.Editor.Tools;

namespace SerenityAITranslator.Editor.Session.Repositories
{
    public class SessionRepository
    {
        public SessionData SessionData { get; private set; }
        
        public async Task SaveSession()
        {
            await FileDataManager.SaveJsonAsync(SessionData, "Session.json");
        }
        
        public async Task LoadSession()
        {
            if (FileDataManager.FileExists("Session.json"))
                SessionData = await FileDataManager.LoadJsonAsync<SessionData>("Session.json");
            else
            {
                SessionData = new SessionData();
                await SaveSession();
            }
        }
        
        public void ClearSession()
        {
            SessionData = new SessionData();
        }   
    }
}