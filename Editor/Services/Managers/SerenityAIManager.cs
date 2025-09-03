using System;
using System.Threading.Tasks;
using SerenityAITranslator.Editor.Services.Common.PromtFactories;
using SerenityAITranslator.Editor.Services.Translation.Context;
using SerenityAITranslator.Editor.Services.Translation.Managers;
using SerenityAITranslator.Editor.Session.Models;
using SerenityAITranslator.Editor.Session.Repositories;
using SerenityAITranslator.Editor.Windows;
using UnityEditor;

namespace SerenityAITranslator.Editor.Services.Managers
{
    public class SerenityAIManager : ISerenityAIManager
    {
        public SessionData Session => _sessionRepository.SessionData;
        public TranslateManager TranslateManager => _translateManager;

        private SerenityAiWindow _window;
        private SessionRepository _sessionRepository;
        private TranslateManager _translateManager;
        
        public async Task Init()
        {
            try
            {
                _sessionRepository = new SessionRepository();
                
                await LoadOrCreateSessionAsync();
                
                _translateManager = new TranslateManager(_sessionRepository);
                
                await SetTranslateContext();
            
                _window = EditorWindow.GetWindow<SerenityAiWindow>("Serenity AI Window");
                _window.Init(this);
                _window.Show();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        
        public void SaveSession()
        {
            _ = _sessionRepository.SaveSession();
        }

        private async Task LoadOrCreateSessionAsync()
        {
            await _sessionRepository.LoadSession();
        }
        
        private async Task SetTranslateContext()
        {
            var context = new TranslatedContext();
            context.BaseLanguage = "English";
            context.PromtFactory = new PromtFactorySimple();
            
            await _translateManager.SetContext(context);
        }
    }
}