using System;
using Xunit;
using ImageProcessingApp;

namespace ImageProcessingApp.Tests
{
    public class ImageProcessingIntegrationTests : IDisposable
    {
        private readonly ImageProcessingService _service;

        public ImageProcessingIntegrationTests()
        {
            // Перед кожним тестом скидаємо стан Singleton для чистоти інтеграції
            ImageProcessingService.ResetForTesting();
            _service = ImageProcessingService.Instance;
        }

        public void Dispose()
        {
            ImageProcessingService.ResetForTesting();
        }

        // =================================================================
        // ПОЗИТИВНІ СЦЕНАРІЇ (Positive Scenarios)
        // =================================================================

        [Fact]
        public void Scenario1_FullPipeline_Grayscale_ShouldSucceed()
        {
            // Arrange (Групування компонентів: Singleton + Factory + Strategy + Observer)
            var factory = new GrayscaleFactory();
            _service.SetFactory(factory);
            
            string receivedNotification = null;
            _service.Publisher.ImageProcessed += (res) => receivedNotification = res;

            string targetImage = "photo.png";

            // Act
            string result = _service.ProcessImage(targetImage);

            // Assert
            Assert.Equal("photo.png_grayscale", result); // Перевірка стратегії та фабрики
            Assert.Equal("photo.png_grayscale", receivedNotification); // Перевірка спостерігача
        }

        [Fact]
        public void Scenario2_RuntimeStrategySwitching_ShouldNotifyCorrectly()
        {
            // Arrange
            string lastNotification = null;
            _service.Publisher.ImageProcessed += (res) => lastNotification = res;

            // Act & Assert 1: Спочатку Grayscale
            _service.SetFactory(new GrayscaleFactory());
            string res1 = _service.ProcessImage("img.jpg");
            Assert.Equal("img.jpg_grayscale", res1);
            Assert.Equal("img.jpg_grayscale", lastNotification);

            // Act & Assert 2: Динамічна зміна на Sepia в рантаймі
            _service.SetFactory(new SepiaFactory());
            string res2 = _service.ProcessImage("img.jpg");
            Assert.Equal("img.jpg_sepia", res2);
            Assert.Equal("img.jpg_sepia", lastNotification);
        }

        [Fact]
        public void Scenario3_SingletonStateStability_AcrossMultipleCallsAndInstances()
        {
            // Arrange
            var factory = new SepiaFactory();
            _service.SetFactory(factory);

            // Отримуємо нібито "інше" посилання на Singleton в іншій частині програми
            ImageProcessingService secondReference = ImageProcessingService.Instance;

            // Act
            string result = secondReference.ProcessImage("test.bmp");

            // Assert
            Assert.Same(_service, secondReference); // Об'єкти ідентичні
            Assert.Equal("test.bmp_sepia", result); // Конфігурація фабрики збереглася в синглтоні
        }

        // =================================================================
        // НЕГАТИВНІ / ГРАНИЧНІ СЦЕНАРІЇ (Negative/Edge Cases)
        // =================================================================

        [Fact]
        public void Scenario4_ProcessingWithoutFactoryConfigured_ShouldThrowInvalidOperationException()
        {
            // Arrange: Фабрику НЕ встановлено (CurrentFactory == null)
            string targetImage = "vacation.png";

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() => 
                _service.ProcessImage(targetImage)
            );

            Assert.Equal("Filter factory is not configured", exception.Message);
        }

        [Fact]
        public void Scenario5_EmptyOrNullImageName_ShouldThrowArgumentException()
        {
            // Arrange
            _service.SetFactory(new GrayscaleFactory());

            // Act & Assert для null
            Assert.Throws<ArgumentException>(() => _service.ProcessImage(null));

            // Act & Assert для порожнього рядка
            Assert.Throws<ArgumentException>(() => _service.ProcessImage(""));
        }
    }
}