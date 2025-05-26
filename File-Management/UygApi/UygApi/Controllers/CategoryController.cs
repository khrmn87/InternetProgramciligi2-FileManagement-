using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UygApi.DTOs;
using UygApi.Models;
using UygApi.Repositories;

namespace UygApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CategoryController : ControllerBase
    {
        private readonly CategoryRepository _categoryRepository;
        private readonly FileModalRepository _fileModalRepository;
        private readonly IMapper _mapper;
        ResultDto _result = new ResultDto();
        public CategoryController(IMapper mapper, CategoryRepository categoryRepository, FileModalRepository fileModalRepository)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _fileModalRepository = fileModalRepository;
        }

        [HttpGet]
        public async Task<List<CategoryDto>> List()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var CategoryDtos = _mapper.Map<List<CategoryDto>>(categories);
            return CategoryDtos;
        }

        [HttpGet("{id}")]
        public async Task<CategoryDto> GetById(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            var CategoryDto = _mapper.Map<CategoryDto>(category);
            return CategoryDto;
        }



        [HttpGet("{id}/Files")]
        public async Task<List<FileModalDto>> ProductList(int id)
        {
            var filesModal = await _fileModalRepository.Where(s => s.CategoryId == id).Include(i => i.Category).ToListAsync();
            var filesModalDtos = _mapper.Map<List<FileModalDto>>(filesModal);
            return filesModalDtos;
        }
        [HttpPost]
        public async Task<ResultDto> Add(CategoryDto model)
        {
            var list = _categoryRepository.Where(s => s.Name == model.Name).ToList();
            if (list.Count() > 0)
            {
                _result.Status = false;
                _result.Message = "Girilen Kategori Adı Kayıtlıdır!";
                return _result;
            }
            var category = _mapper.Map<Category>(model);
            category.Created = DateTime.Now;
            category.Updated = DateTime.Now;
            await _categoryRepository.AddAsync(category);
            _result.Status = true;
            _result.Message = "Kayıt Eklendi";
            return _result;
        }
        [HttpPut]
        public async Task<ResultDto> Update(Category model)
        {
            var category = await _categoryRepository.GetByIdAsync(model.Id);
            category.Name = model.Name;
            category.IsActive = model.IsActive;
            category.Updated = DateTime.Now;
            await _categoryRepository.UpdateAsync(category);
            _result.Status = true;
            _result.Message = "Kayıt GüncelLendi";
            return _result;
        }

        [HttpDelete("{id}")]
        public async Task<ResultDto> Delete(int id)
        {
            var list = await _fileModalRepository.Where(s => s.CategoryId == id).ToListAsync();
            if (list.Count() > 0)
            {
                _result.Status = false;
                _result.Message = "Üzerine Dosya Kaydı Olan Kategori Silinemez!";
                return _result;
            }

            await _categoryRepository.DeleteAsync(id);
            _result.Status = true;
            _result.Message = "Kayıt Silindi";
            return _result;

        }
    }
}
