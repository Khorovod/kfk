Приложение запускается в docker

Генерация через avrogen схемы order.avsc
Схемы в проекте Contracts
Consumer десериализирует сообщение с помощью SpecificDatumReader. Реализуется гарантия at least once через ручной ack после обработки сообщения.
