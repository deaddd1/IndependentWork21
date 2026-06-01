using System;

namespace ImageProcessingApp
{
    // === 1. STRATEGY ===
    public interface IFilterStrategy
    {
        string ApplyFilter(string imageName);
    }

    public class GrayscaleFilterStrategy : IFilterStrategy
    {
        public string ApplyFilter(string imageName) => $"{imageName}_grayscale";
    }

    public class SepiaFilterStrategy : IFilterStrategy
    {
        public string ApplyFilter(string imageName) => $"{imageName}_sepia";
    }

    // === 2. FACTORY METHOD ===
    public abstract class FilterFactory
    {
        public abstract IFilterStrategy CreateFilter();
    }

    public class GrayscaleFactory : FilterFactory
    {
        public override IFilterStrategy CreateFilter() => new GrayscaleFilterStrategy();
    }

    public class SepiaFactory : FilterFactory
    {
        public override IFilterStrategy CreateFilter() => new SepiaFilterStrategy();
    }

    // === 3. OBSERVER ===
    public class ImagePublisher
    {
        public event Action<string> ImageProcessed;

        public void Notify(string result)
        {
            ImageProcessed?.Invoke(result);
        }
    }

    // === 4. SINGLETON MANAGER ===
    public class ImageProcessingService
    {
        private static ImageProcessingService _instance;
        private static readonly object _lock = new object();

        public FilterFactory CurrentFactory { get; private set; }
        public ImagePublisher Publisher { get; } = new ImagePublisher();

        private ImageProcessingService() { }

        public static ImageProcessingService Instance
        {
            get
            {
                lock (_lock)
                {
                    return _instance ??= new ImageProcessingService();
                }
            }
        }

        // Для тестів: метод скидання стану Singleton
        public static void ResetForTesting()
        {
            lock (_lock)
            {
                _instance = null;
            }
        }

        public void SetFactory(FilterFactory factory)
        {
            CurrentFactory = factory;
        }

        public string ProcessImage(string imageName)
        {
            if (string.IsNullOrEmpty(imageName))
                throw new ArgumentException("Image name cannot be null or empty");

            if (CurrentFactory == null)
                throw new InvalidOperationException("Filter factory is not configured");

            // Factory Method створює Strategy
            IFilterStrategy filter = CurrentFactory.CreateFilter();
            
            // Виконання стратегії
            string result = filter.ApplyFilter(imageName);

            // Observer сповіщає підписників
            Publisher.Notify(result);

            return result;
        }
    }
}