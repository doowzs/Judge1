import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ContestMode, ContestViewDto } from '../../../interfaces/contest.interfaces';
import { ContestService } from '../../../services/contest.service';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-contest-description',
  templateUrl: './description.component.html',
  styleUrls: ['./description.component.css']
})
export class ContestDescriptionComponent implements OnInit {
  public contestId: number;
  public contest: ContestViewDto;

  constructor(
    private title: Title,
    private route: ActivatedRoute,
    private service: ContestService
  ) {
    this.contestId = this.route.snapshot.params.contestId;
  }

  ngOnInit() {
    this.service.getSingle(this.contestId)
      .subscribe(contest => {
        this.contest = contest;
        this.title.setTitle(contest.title);
      });
  }
}