using Domain.Entities;
using Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DataSeeds
{
    public static class LocationSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            // ===== SEED DISTRICTS =====
            if (!await context.Districts.AnyAsync())
            {
                var thuDuc = new District
                {
                    Id = Guid.NewGuid(),
                    Name = "Thành phố Thủ Đức",
                    Code = "THU_ĐUC",
                    ProvinceCode = "HCM"
                };

                var quan1 = new District
                {
                    Id = Guid.NewGuid(),
                    Name = "Quận 1",
                    Code = "QUAN_1",
                    ProvinceCode = "HCM"
                };

                var quan3 = new District
                {
                    Id = Guid.NewGuid(),
                    Name = "Quận 3",
                    Code = "QUAN_3",
                    ProvinceCode = "HCM"
                };

                var binhThanh = new District
                {
                    Id = Guid.NewGuid(),
                    Name = "Bình Thạnh",
                    Code = "BINH_THANH",
                    ProvinceCode = "HCM"
                };

                var goVap = new District
                {
                    Id = Guid.NewGuid(),
                    Name = "Gò Vấp",
                    Code = "GO_VAP",
                    ProvinceCode = "HCM"
                };

                var hoChiMinh = new District
                {
                    Id = Guid.NewGuid(),
                    Name = "Hồ Chí Minh",
                    Code = "HCM",
                    ProvinceCode = "HCM"
                };
                await context.Districts.AddRangeAsync(
                    thuDuc,
                    quan1,
                    quan3,
                    binhThanh,
                    goVap,
                    hoChiMinh
                );

                await context.SaveChangesAsync();

                // ===== SEED WARDS =====
                if (!await context.Wards.AnyAsync())
                {
                    var wards = new List<Ward>
                    {
                        // ===== THU DUC =====
                        new Ward { Id = Guid.NewGuid(), Name = "Linh Trung", Code = "HCM-PHUONG_LINH_TRUNG", DistrictId = thuDuc.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Linh Tây", Code = "HCM-PHUONG_LINH_TAY", DistrictId = thuDuc.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Linh Chiểu", Code = "HCM-PHUONG_LINH_CHIEU", DistrictId = thuDuc.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Bình Thọ", Code = "HCM-PHUONG_BINH_THO", DistrictId = thuDuc.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Trường Thọ", Code = "HCM-PHUONG_TRUONG_THO", DistrictId = thuDuc.Id },

                        // ===== QUAN 1 =====
                        new Ward { Id = Guid.NewGuid(), Name = "Bến Nghé", Code = "HCM-PHUONG_BEN_NGHE", DistrictId = quan1.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Bến Thành", Code = "HCM-PHUONG_BEN_THANH", DistrictId = quan1.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Đa Kao", Code = "HCM-PHUONG_DA_KAO", DistrictId = quan1.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Nguyễn Thái Bình", Code = "HCM-PHUONG_NGUYEN_THAI_BINH", DistrictId = quan1.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Phạm Ngũ Lão", Code = "HCM-PHUONG_PHAM_NGU_LAO", DistrictId = quan1.Id },

                        // ===== QUAN 3 =====
                        new Ward { Id = Guid.NewGuid(), Name = "Phường 1", Code = "HCM-PHUONG_1_Q3", DistrictId = quan3.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Phường 2", Code = "HCM-PHUONG_2_Q3", DistrictId = quan3.Id },

                        // ===== BINH THANH =====
                        new Ward { Id = Guid.NewGuid(), Name = "Phường 1 Bình Thạnh", Code = "HCM-PHUONG_1_BT", DistrictId = binhThanh.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Phường 2 Bình Thạnh", Code = "HCM-PHUONG_2_BT", DistrictId = binhThanh.Id },

                        // ===== GO VAP =====
                        new Ward { Id = Guid.NewGuid(), Name = "Phường 1 Gò Vấp", Code = "HCM-PHUONG_1_GV", DistrictId = goVap.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Phường 2 Gò Vấp", Code = "HCM-PHUONG_2_GV", DistrictId = goVap.Id },
                        new Ward { Id = Guid.NewGuid(), Name = "Phường Phú Thọ Hòa", Code = "HCM-PHUONG_PHU_THO_HOA", DistrictId = goVap.Id }
                    };

                    await context.Wards.AddRangeAsync(wards);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}