using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Podcast.BLL.Data;
using Podcast.BLL.Services.Contracts;
using Podcast.BLL.ViewModels.TopicViewModels;
using Podcast.MVC.Extentions;
using System.Security.Principal;

namespace Podcast.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[AutoValidateAntiforgeryToken]
public class TopicController : Controller
{
    private readonly ITopicService _topicService;
    private readonly IMapper _mapper;

    public TopicController(ITopicService topicService, IMapper mapper)
    {
        _topicService = topicService;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        var topics = await _topicService.GetListAsync(include: x => x.Include(y => y.Episodes!));

        return View(topics);
    }


    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(TopicCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!model.CoverFile.CheckType())
        {
            ModelState.AddModelError("CoverFile", "Please add image format");
            return View(model);
        }

        if (!model.CoverFile.CheckSize(2))
        {
            ModelState.AddModelError("CoverFile", "Size should be <= 2 mb");
            return View(model);
        }

        model.CoverUrl = await model.CoverFile.GenerateFileAsync(Constants.TopicImagePath);

        await _topicService.CreateAsync(model);

        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Update(int id)
    {
        var topic = await _topicService.GetAsync(id);

        if (topic == null) return NotFound();

        var result = _mapper.Map<TopicUpdateViewModel>(topic);
        return View(result);
    }

    [HttpPost]
    public async Task<IActionResult> Update(TopicUpdateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        #region ImageValidations

        if (model.CoverFile != null)
        {
            if (!model.CoverFile.CheckType())
            {
                ModelState.AddModelError("CoverFile", "Please add image format");
                return View(model);
            }

            if (!model.CoverFile.CheckSize(2))
            {
                ModelState.AddModelError("CoverFile", "Size should be <= 2 mb");
                return View(model);
            }
            model.CoverUrl = await model.CoverFile.GenerateFileAsync(Constants.TopicImagePath);
        }
        #endregion



        await _topicService.UpdateAsync(model);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
       

        await _topicService.RemoveAsync(id);

        return RedirectToAction(nameof(Index));
    }

}
