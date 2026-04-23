# 🇹🇷 Türkçe

# HotelAppDDD

.NET 8 ile geliştirilmiş, **Domain-Driven Design (DDD)** prensiplerine dayanan bir otel rezervasyon sistemi. Proje şu an yalnızca Domain katmanını ve kapsamlı unit testleri içermektedir.

---

## Proje Yapısı

```
HotelAppDDD/
├── HotelAppDDD.Domain/          # Domain katmanı (iş kuralları)
│   ├── Common/
│   │   ├── AggregateRoot.cs     # Tüm aggregate'lerin base sınıfı
│   │   └── IDomainEvent.cs      # Domain event arayüzü (MediatR INotification)
│   ├── Room/
│   │   ├── Room.cs              # Room aggregate root
│   │   └── Events/
│   │       └── RoomEvents.cs    # RoomCreated, RoomReserved, RoomReleased
│   ├── Reservation/
│   │   ├── Reservation.cs       # Reservation aggregate root
│   │   └── Events/
│   │       └── ReservationEvents.cs  # ReservationCreated, Cancelled, CheckedIn, CheckedOut
│   ├── Interfaces/
│   │   ├── IRoomRepository.cs
│   │   ├── IReservationRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Exceptions/
│       └── DomainExceptions.cs  # BusinessRuleException, NotFoundException, UnauthorizedException
└── HotelAppDDD.Tests/           # Unit test projesi
    ├── RoomTests.cs             # 14 test
    └── ReservationTests.cs      # 22 test
```

---

## Domain Modeli

### Room (Oda)

Bir odanın oluşturulmasını, rezerve edilmesini ve serbest bırakılmasını yönetir.

| Özellik      | Tip    | Açıklama                      |
|--------------|--------|-------------------------------|
| Id           | Guid   | Benzersiz kimlik              |
| RoomNumber   | int    | Oda numarası (pozitif olmalı) |
| RoomType     | string | Oda tipi (boş olamaz)         |
| Capacity     | int    | Kapasite (pozitif olmalı)     |
| IsAvailable  | bool   | Müsaitlik durumu              |

**İş Kuralları:**
- Oda numarası ve kapasite sıfırdan büyük olmalıdır.
- Oda tipi boş bırakılamaz.
- Müsait olmayan oda tekrar rezerve edilemez.
- Talep edilen misafir sayısı odanın kapasitesini geçemez.
- Zaten müsait olan oda serbest bırakılamaz.

**Domain Events:** `RoomCreatedEvent`, `RoomReservedEvent`, `RoomReleasedEvent`

---

### Reservation (Rezervasyon)

Rezervasyon oluşturma, iptal, check-in ve check-out süreçlerini yönetir.

| Özellik                | Tip       | Açıklama                            |
|------------------------|-----------|-------------------------------------|
| Id                     | Guid      | Benzersiz kimlik                    |
| RoomId                 | Guid      | İlgili oda                          |
| GuestId                | Guid      | Misafir kimliği                     |
| ExpectedCheckInDate    | DateTime  | Planlanan giriş tarihi              |
| ExpectedCheckOutDate   | DateTime  | Planlanan çıkış tarihi              |
| ActualCheckInDate      | DateTime? | Gerçekleşen giriş tarihi            |
| ActualCheckOutDate     | DateTime? | Gerçekleşen çıkış tarihi            |
| IsActive               | bool      | Rezervasyon aktif mi                |

**İş Kuralları:**
- Rezervasyon en az 1 gün, en fazla 30 gün olabilir.
- Check-in yapılmış rezervasyon iptal edilemez.
- İptal edilmiş rezervasyon tekrar iptal edilemez.
- Check-in yapmadan check-out yapılamaz.
- Aynı rezervasyona iki kez check-in / check-out yapılamaz.

**Geç Çıkış Ücreti:** `CalculateLateFee()` — planlanan çıkış tarihini aşan her gün için **2.000** ücret hesaplar.

**Domain Events:** `ReservationCreatedEvent`, `ReservationCancelledEvent`, `ReservationCheckedInEvent`, `ReservationCheckedOutEvent`

---

## Mimari Kararlar

### Aggregate Root

Her aggregate, `AggregateRoot` base sınıfından türer. Bu sınıf:
- Domain event listesini tutar (`DomainEvents`)
- `AddDomainEvent` ile event kaydeder
- `ClearDomainEvents` ile event listesini temizler

```csharp
public abstract class AggregateRoot
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }
    protected void AddDomainEvent(IDomainEvent domainEvent);
    public void ClearDomainEvents();
}
```

### Domain Events

Domain event'ler `MediatR.INotification` arayüzünü uygular. Bu sayede Infrastructure katmanında `INotificationHandler<T>` ile kolayca dinlenebilir.

### Repository Pattern

Domain katmanı yalnızca arayüzleri tanımlar; implementasyonlar Infrastructure katmanına aittir.

```csharp
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    void Update(Room room);
    void Delete(Room room);
}
```

### Unit of Work

Birden fazla repository değişikliğini tek bir transaction'a sarmak için kullanılır.

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

---

## Testler

Proje **xUnit** ve **FluentAssertions** kütüphaneleriyle test edilmektedir.

```bash
dotnet test
```

**Toplam: 36 test — 36 geçti**

| Test Dosyası            | Test Sayısı | Kapsam                                          |
|-------------------------|-------------|--------------------------------------------------|
| `RoomTests.cs`          | 14          | Oluşturma, rezervasyon, serbest bırakma          |
| `ReservationTests.cs`   | 22          | Oluşturma, iptal, check-in, check-out, geç ücret |

**Kullanılan test pattern'leri:**

```csharp
// Happy path
[Fact]
public void Create_WithValidDates_ShouldReturnActiveReservation() { ... }

// İş kuralı ihlali
[Fact]
public void Cancel_AfterCheckIn_ShouldThrow() { ... }

// Domain event kontrolü
[Fact]
public void Create_ShouldRaiseReservationCreatedEvent() { ... }

// Çoklu parametre
[Theory]
[InlineData(0)]
[InlineData(-1)]
public void Create_WithInvalidCapacity_ShouldThrow(int capacity) { ... }
```

---

## Kurulum ve Çalıştırma

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Derleme

```bash
dotnet build
```

### Testleri Çalıştırma

```bash
dotnet test
```

---

## Kullanılan Teknolojiler

| Teknoloji           | Versiyon | Kullanım Amacı             |
|---------------------|----------|----------------------------|
| .NET                | 8.0      | Hedef framework            |
| MediatR             | —        | Domain event arayüzü       |
| xUnit               | 2.5.3    | Unit test framework        |
| FluentAssertions    | 8.0.0    | Okunabilir test assertion  |
| coverlet            | 6.0.0    | Test kapsam raporu         |

---

## Gelecek Planlar

- [ ] Application katmanı (CQRS, MediatR handler'ları)
- [ ] Infrastructure katmanı (EF Core, PostgreSQL)
- [ ] API katmanı (ASP.NET Core Web API)
- [ ] Domain event handler'ları
- [ ] Entegrasyon testleri

# EN English
# HotelAppDDD

A hotel reservation system built with **.NET 8**, following **Domain-Driven Design (DDD)** principles. The project currently contains only the Domain layer and comprehensive unit tests.

---

## Project Structure

```
HotelAppDDD/
├── HotelAppDDD.Domain/          # Domain layer (business rules)
│   ├── Common/
│   │   ├── AggregateRoot.cs     # Base class for all aggregates
│   │   └── IDomainEvent.cs      # Domain event interface (MediatR INotification)
│   ├── Room/
│   │   ├── Room.cs              # Room aggregate root
│   │   └── Events/
│   │       └── RoomEvents.cs    # RoomCreated, RoomReserved, RoomReleased
│   ├── Reservation/
│   │   ├── Reservation.cs       # Reservation aggregate root
│   │   └── Events/
│   │       └── ReservationEvents.cs  # ReservationCreated, Cancelled, CheckedIn, CheckedOut
│   ├── Interfaces/
│   │   ├── IRoomRepository.cs
│   │   ├── IReservationRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Exceptions/
│       └── DomainExceptions.cs  # BusinessRuleException, NotFoundException, UnauthorizedException
└── HotelAppDDD.Tests/           # Unit test project
    ├── RoomTests.cs             # 14 tests
    └── ReservationTests.cs      # 22 tests
```

---

## Domain Model

### Room

Manages room creation, reservation, and release.

| Property     | Type   | Description                        |
|--------------|--------|------------------------------------|
| Id           | Guid   | Unique identifier                  |
| RoomNumber   | int    | Room number (must be positive)     |
| RoomType     | string | Room type (cannot be empty)        |
| Capacity     | int    | Capacity (must be positive)        |
| IsAvailable  | bool   | Availability status                |

**Business Rules:**
- Room number and capacity must be greater than zero.
- Room type cannot be empty.
- An unavailable room cannot be reserved again.
- The requested number of guests cannot exceed the room's capacity.
- A room that is already available cannot be released.

**Domain Events:** `RoomCreatedEvent`, `RoomReservedEvent`, `RoomReleasedEvent`

---

### Reservation

Manages reservation creation, cancellation, check-in, and check-out.

| Property               | Type      | Description                         |
|------------------------|-----------|-------------------------------------|
| Id                     | Guid      | Unique identifier                   |
| RoomId                 | Guid      | Associated room                     |
| GuestId                | Guid      | Guest identifier                    |
| ExpectedCheckInDate    | DateTime  | Planned check-in date               |
| ExpectedCheckOutDate   | DateTime  | Planned check-out date              |
| ActualCheckInDate      | DateTime? | Actual check-in date                |
| ActualCheckOutDate     | DateTime? | Actual check-out date               |
| IsActive               | bool      | Whether the reservation is active   |

**Business Rules:**
- A reservation must be at least 1 day and at most 30 days long.
- A reservation that has been checked in cannot be cancelled.
- A cancelled reservation cannot be cancelled again.
- Check-out cannot occur without a prior check-in.
- A reservation cannot be checked in or checked out twice.

**Late Checkout Fee:** `CalculateLateFee()` — charges **2,000** per day beyond the planned checkout date.

**Domain Events:** `ReservationCreatedEvent`, `ReservationCancelledEvent`, `ReservationCheckedInEvent`, `ReservationCheckedOutEvent`

---

## Architectural Decisions

### Aggregate Root

Every aggregate inherits from the `AggregateRoot` base class. This class:
- Holds the list of domain events (`DomainEvents`)
- Records events via `AddDomainEvent`
- Clears the event list via `ClearDomainEvents`

```csharp
public abstract class AggregateRoot
{
    public IReadOnlyList<IDomainEvent> DomainEvents { get; }
    protected void AddDomainEvent(IDomainEvent domainEvent);
    public void ClearDomainEvents();
}
```

### Domain Events

Domain events implement `MediatR.INotification`, allowing them to be easily consumed in the Infrastructure layer via `INotificationHandler<T>`.

### Repository Pattern

The Domain layer only defines interfaces; implementations belong to the Infrastructure layer.

```csharp
public interface IRoomRepository
{
    Task<Room?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Room room, CancellationToken ct = default);
    void Update(Room room);
    void Delete(Room room);
}
```

### Unit of Work

Used to wrap changes across multiple repositories into a single transaction.

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

---

## Tests

The project is tested using **xUnit** and **FluentAssertions**.

```bash
dotnet test
```

**Total: 36 tests — 36 passed**

| Test File               | Test Count | Coverage                                          |
|-------------------------|------------|---------------------------------------------------|
| `RoomTests.cs`          | 14         | Creation, reservation, release                    |
| `ReservationTests.cs`   | 22         | Creation, cancellation, check-in, check-out, late fee |

**Test patterns used:**

```csharp
// Happy path
[Fact]
public void Create_WithValidDates_ShouldReturnActiveReservation() { ... }

// Business rule violation
[Fact]
public void Cancel_AfterCheckIn_ShouldThrow() { ... }

// Domain event verification
[Fact]
public void Create_ShouldRaiseReservationCreatedEvent() { ... }

// Multiple parameters
[Theory]
[InlineData(0)]
[InlineData(-1)]
public void Create_WithInvalidCapacity_ShouldThrow(int capacity) { ... }
```

---

## Setup & Running

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

---

## Technologies Used

| Technology          | Version  | Purpose                        |
|---------------------|----------|--------------------------------|
| .NET                | 8.0      | Target framework               |
| MediatR             | —        | Domain event interface         |
| xUnit               | 2.5.3    | Unit test framework            |
| FluentAssertions    | 8.0.0    | Readable test assertions       |
| coverlet            | 6.0.0    | Test coverage reporting        |

---

## Roadmap

- [ ] Application layer (CQRS, MediatR handlers)
- [ ] Infrastructure layer (EF Core, PostgreSQL)
- [ ] API layer (ASP.NET Core Web API)
- [ ] Domain event handlers
- [ ] Integration tests
