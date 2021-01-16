zipper
=============
###### *Trying to create file archiver by myself.*

### TL;DR

Библиотека + консолька для поблочного сжатия и распаковки файлов с помощью GzipStream:
- режем файл на кусочки, сжимаем независимо и в мультитреде;
- умеет архивировать файлы, которые не влазят в память;
- есть возможность ограничить используемый размер памяти, количество потоков для обработки и вот это вот всё;
- CLI.

### Сабж

В целом, идея реализации простая - типичный читатель-писатель. 

Читатель читает поток, есть еще кучка воркеров для обработки того, что считано, писатель пишет в выходной поток.

Формат получился самописный, GZip использовался для сжатия и рапсаковки данных.
Хранится всё в простеньких Batch'ах. 

При архивации сначала пишется размер упакованного Batch'а, а потом сам Batch - запакованные данные.

Разархивация аналогично-зеркальна - сначала читается размер Batch'а, а потом считываются данные.

### CLI

Для работы с библиотекой приложен небольшой проект с CLI. Команды максимально простые.

Команда | Описание
-- | --
`compress <input> <output>` | Архивирует файл.
`decompress <input> <output>` | Разархивирует файл.

Есть еще небольшие плюшки к этому:
Доп. команда (кратко) | Доп. команда (полно) | Описание
-- | -- | --
`-b` | `--buffer` | BufferSize - только при упаковке, размер читаемого блока из потока.
`-t` | `--threads` | ThreadsCount - количество тредов, которые будут упаковывать/распаковывать прочитанные блоки.
`-l` | `--limit` | WorkLimit - максимальное количество Batch'ей, которые будут считаны. Если число достигнуто, то читатель просто будет ждать, пока их не обработают.
`-v` | `--verbose` | Verbose - пишем инфу о упаковке/распаковке в консоль.

### API

Основным классом является `StreamPipeline`, при инициализации которого можно указать количество рабочих потоков и максимальное количество обрабатываемых элементов.

`StreamPipeline` предоставляет fluent-like API для настройки. Обязательно указать `Reader` и `Writer` - классы, реализующие `IReader` и `IWriter` для чтения и записи даннных соотвественно. Также есть возможность подписаться на события чтения и записи блоков. Для более гибкого контроля можно передать `Converter`, который будет использоваться для обработки данных, например, архивации или разархивации.

Данные должны быть прочитаны и записаны одним и тем же типом reader/writer, например, если для записи использовался `BatchStreamWriter`, то для разархивации должен использоваться `BatchStreamReader` и *vice versa*.

Упаковка:
```csharp
using(var inputStream = new FileStream("input.file", FileMode.Open))
using(var outputStream = new FileStream("compressed.file", FileMode.Create))
using(var pipeline = new StreamPipeline(16, 16))
{
  pipeline.OnRead += (sender, args) => Console.WriteLine(args.Message);
  pipeline.OnWrite += (sender, args) => Console.WriteLine(args.Message);

  pipeline
    .Reader(new FileStreamReader(1024 * 1024))
    .Writer(new BatchStreamWriter())
    .Converter(new GzipCompressor())
    .Proceed(inputStream, outputStream);
}
```

Распаковка:
```csharp
using(var inputStream = new FileStream("compressed.file", FileMode.Open))
using(var outputStream = new FileStream("decompressed.file", FileMode.Create))
using(var pipeline = new StreamPipeline(16, 16))
{
  pipeline.OnRead += (sender, args) => Console.WriteLine(args.Message);
  pipeline.OnWrite += (sender, args) => Console.WriteLine(args.Message);

  pipeline
    .Reader(new BatchStreamReader())
    .Writer(new FileStreamWriter())
    .Converter(new GzipDecompressor())
    .Proceed(inputStream, outputStream);
}
```

### TODO

- [x] producer/consumer/workers обвязка;
- [x] gzip compressor/decompressor;
- [x] blob reader/writer;
- [x] filestream reader/writer;
- [x] очередь с приоритетом для писателя;
- [x] CLI;
- [ ] кучка тестов;
- [ ] кучка зеленых тестов.
