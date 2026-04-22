using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoomEntity = HotelAppDDD.Domain.Room.Room;

namespace HotelAppDDD.Domain.Interfaces;

    public interface IRoomRepository
    {
        Task<RoomEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(RoomEntity room, CancellationToken ct = default);
        void Update(RoomEntity room);
        void Delete(RoomEntity room);
}

