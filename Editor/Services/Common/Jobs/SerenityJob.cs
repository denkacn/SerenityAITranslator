using System.Collections.Generic;

namespace SerenityAITranslator.Editor.Services.Common.Jobs
{
    public class SerenityJob
    {
        public string Name { get; private set; }
        public SerenityJobStatus Status { get; private set; } = SerenityJobStatus.Idle;
        public int Current { get; private set; }
        public int Total { get; private set; }
        public string CurrentItem { get; private set; }
        public string ErrorMessage { get; private set; }
        public List<string> Errors { get; } = new List<string>();

        public bool IsRunning => Status == SerenityJobStatus.Running;
        public float Progress => Total <= 0 ? 0f : (float)Current / Total;

        public void Begin(string name, int total)
        {
            Name = name;
            Total = total < 0 ? 0 : total;
            Current = 0;
            CurrentItem = string.Empty;
            ErrorMessage = string.Empty;
            Errors.Clear();
            Status = SerenityJobStatus.Running;
        }

        public void Step(string currentItem = null)
        {
            if (Status != SerenityJobStatus.Running) return;
            Current++;
            CurrentItem = currentItem ?? string.Empty;
        }

        public void AddError(string error)
        {
            if (string.IsNullOrEmpty(error)) return;
            ErrorMessage = error;
            Errors.Add(error);
        }

        public void Complete()
        {
            Status = Errors.Count > 0 ? SerenityJobStatus.Failed : SerenityJobStatus.Completed;
        }

        public void Cancel()
        {
            Status = SerenityJobStatus.Canceled;
        }

        public void Fail(string error)
        {
            AddError(error);
            Status = SerenityJobStatus.Failed;
        }
    }
}
