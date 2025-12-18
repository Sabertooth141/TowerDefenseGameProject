using System;

namespace EventSystem
{
    public class TaskEventHub
    {
        // FILE UPLOAD
        // verification
        public static event Action OnVerification;
        public static event Action<int> OnVerificationResult;
        
        // upload
        public static event Action OnStartUpload;
        public static event Action<bool> OnUploadResult;
        
        
        // TRIGGER FUNCTIONS
        public static void TriggerOnVerification()
        {
            OnVerification?.Invoke();
        }
        
        // if fileSize = -1 => not valid
        public static void TriggerOnVerificationResult(int fileSize)
        {
            OnVerificationResult?.Invoke(fileSize);
        }

        public static void TriggerOnStartUpload()
        {
            OnStartUpload?.Invoke();
        }

        public static void TriggerOnUploadResult(bool result)
        {
            OnUploadResult?.Invoke(result);
        }
    }
}