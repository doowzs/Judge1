﻿import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

import { ProblemService } from '../../../services/problem.service';
import { ProblemViewDto } from '../../../interfaces/problem.interfaces';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-problem-detail',
  templateUrl: './detail.component.html',
  styleUrls: ['./detail.component.css']
})
export class ProblemDetailComponent implements OnInit, OnDestroy {
  public problemId: number;
  public problem: ProblemViewDto;

  public destroy$ = new Subject();

  copyToClipboard = (content: string): void => {
    navigator.clipboard.writeText(content);
  }

  constructor(
    private route: ActivatedRoute,
    private service: ProblemService,
  ) {
    this.route.params
      .pipe(takeUntil(this.destroy$))
      .subscribe(params => {
        this.problemId = params.problemId;
        this.loadProblem();
      });
  }

  ngOnInit() {
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  public loadProblem() {
    this.service.getSingle(this.problemId)
      .subscribe(problem => this.problem = problem);
  }
}

