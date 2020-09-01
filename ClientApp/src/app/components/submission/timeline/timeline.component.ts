﻿import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { forkJoin, interval, Observable, Subject } from 'rxjs';
import { map, take, takeUntil } from 'rxjs/operators';

import { VerdictInfo, VerdictStage } from '../../../consts/verdicts.consts';
import { SubmissionService } from '../../../services/submission.service';
import { PaginatedList } from '../../../interfaces/pagination.interfaces';
import { SubmissionInfoDto } from '../../../interfaces/submission.interfaces';
import { AuthorizeService } from '../../../../api-authorization/authorize.service';

@Component({
  selector: 'app-submission-timeline',
  templateUrl: './timeline.component.html',
  styleUrls: ['./timeline.component.css']
})
export class SubmissionTimelineComponent implements OnInit, OnDestroy {
  @Input() public problemId: number;

  private destroy$ = new Subject();

  public userId: Observable<string>;
  public list: PaginatedList<SubmissionInfoDto>;
  public submissions: SubmissionInfoDto[] = [];

  constructor(
    private auth: AuthorizeService,
    private service: SubmissionService
  ) {
    this.userId = this.auth.getUser().pipe(map(u => u && u.sub));
  }

  ngOnInit() {
    interval(2000)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        if (this.submissions.filter(s => (s.verdict as VerdictInfo).stage === VerdictStage.RUNNING).length > 0) {
          this.updatePendingSubmissions();
        }
      });
    this.service.newSubmission
      .pipe(takeUntil(this.destroy$))
      .subscribe(submission => this.submissions.unshift(submission));
    this.userId.pipe(take(1)).subscribe(userId => {
      this.service.getPaginatedList(null, this.problemId, userId, null, 1)
        .subscribe(list => {
          for (let i = 0; i < list.items.length; ++i) {
            this.submissions.unshift(list.items[i]);
          }
        });
    });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private updatePendingSubmissions(): void {
    const observables: Observable<SubmissionInfoDto>[] = [];
    const pendingSubmissions = this.submissions.filter(s => (s.verdict as VerdictInfo).stage === VerdictStage.RUNNING);
    for (let i = 0; i < pendingSubmissions.length; ++i) {
      const submission = pendingSubmissions[i];
      observables.push(this.service.getSingleAsInfo(submission.id));
    }
    forkJoin(observables).subscribe(updatedSubmissions => {
      for (let i = 0; i < updatedSubmissions.length; ++i) {
        const updated = updatedSubmissions[i];
        const submission = this.submissions.find(s => s.id === updated.id);
        submission.verdict = updated.verdict;
        submission.failedOn = updated.failedOn;
        submission.score = updated.score;
        submission.judgedAt = updated.judgedAt;
      }
    });
  }

  public failedOnSample(submission: SubmissionInfoDto): boolean {
    return (submission.verdict as VerdictInfo).stage === VerdictStage.REJECTED && submission.failedOn === 0;
  }
}
