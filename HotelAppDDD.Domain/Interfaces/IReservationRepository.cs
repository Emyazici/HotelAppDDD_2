
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReservationEntity = HotelAppDDD.Domain.Reservation.Reservation;
namespace HotelAppDDD.Domain.Interfaces;

    public interface IReservationRepository
    {
    Task<ReservationEntity?> GetByIdAsync(Guid Id, CancellationToken ct = default);
    Task<List<ReservationEntity>> GetActiveReservationsByMemberAsync(Guid memberId, CancellationToken ct = default);
    Task AddAsync(ReservationEntity reservation, CancellationToken ct = default);
    void Update(ReservationEntity reservation);
    void Delete(ReservationEntity reservation);
}

