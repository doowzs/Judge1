﻿import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { AuthorizeGuard } from '../api-authorization/authorize.guard';
import { ApiAuthorizationModule } from '../api-authorization/api-authorization.module';

import { AdminGuard } from './admin.guard';
import { AdminComponent } from './admin.component';
import { AdminDashboardComponent } from './components/dashboard/dashboard.component';
import { AdminBulletinListComponent } from './components/bulletin/list/list.component';
import { AdminBulletinFormComponent } from './components/bulletin/form/form.component';
import { AdminBulletinCreatorComponent } from './components/bulletin/creator/creator.component';
import { AdminBulletinEditorComponent } from './components/bulletin/editor/editor.component';
import { AdminUserListComponent } from './components/user/list/list.component';
import { AdminUserFormComponent } from './components/user/form/form.component';
import { AdminUserEditorComponent } from './components/user/editor/editor.component';
import { AdminContestListComponent } from './components/contest/list/list.component';
import { AdminContestFormComponent } from './components/contest/form/form.component';
import { AdminContestCreatorComponent } from './components/contest/creator/creator.component';
import { AdminContestEditorComponent } from './components/contest/editor/editor.component';
import { AdminContestRegistrationsComponent } from './components/contest/registrations/registrations.component';
import { AdminProblemListComponent } from './components/problem/list/list.component';
import { AdminProblemFormComponent } from './components/problem/form/form.component';
import { AdminProblemCreatorComponent } from './components/problem/creator/creator.component';
import { AdminProblemArchiveComponent } from './components/problem/archive/archive.component';
import { AdminProblemEditorComponent } from './components/problem/editor/editor.component';
import { AdminProblemTestCasesComponent } from './components/problem/test-cases/test-cases.component';
import { AdminSubmissionListComponent } from './components/submission/list/list.component';
import { AdminSubmissionFormComponent } from './components/submission/form/form.component';
import { AdminSubmissionEditorComponent } from './components/submission/editor/editor.component';
import { AdminSubmissionRejudgeComponent } from './components/submission/rejudge/rejudge.component';

import { MarkdownModule } from '../lib/markdown/markdown.module';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzPageHeaderModule } from 'ng-zorro-antd/page-header';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzCheckboxModule } from 'ng-zorro-antd/checkbox';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzPopconfirmModule } from 'ng-zorro-antd/popconfirm';
import { NzDividerModule } from 'ng-zorro-antd/divider';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzRadioModule } from 'ng-zorro-antd/radio';
import { NzBreadCrumbModule } from 'ng-zorro-antd/breadcrumb';
import { NzDropDownModule } from 'ng-zorro-antd/dropdown';

@NgModule({
  imports: [
    CommonModule,
    BrowserModule,
    BrowserAnimationsModule,
    ReactiveFormsModule,
    RouterModule.forChild([
      {
        path: 'admin',
        component: AdminComponent,
        canActivate: [AuthorizeGuard, AdminGuard],
        data: { roles: ['*'], breadcrumb: 'Admin' },
        children: [
          { path: '', pathMatch: 'full', component: AdminDashboardComponent },
          {
            path: 'bulletin',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator'], breadcrumb: 'Bulletins' },
            children: [
              { path: '', pathMatch: 'full', component: AdminBulletinListComponent },
              { path: 'new', component: AdminBulletinCreatorComponent, data: { breadcrumb: 'Create' } },
              {
                path: ':bulletinId',
                component: AdminBulletinEditorComponent,
                data: { breadcrumb: 'View' }
              },
            ]
          },
          {
            path: 'user',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator', 'UserManager'], breadcrumb: 'Users' },
            children: [
              { path: '', pathMatch: 'full', component: AdminUserListComponent },
              { path: ':userId', component: AdminUserEditorComponent, data: { breadcrumb: 'View' } }
            ]
          },
          {
            path: 'contest',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator', 'ContestManager'], breadcrumb: 'Contests' },
            children: [
              { path: '', pathMatch: 'full', component: AdminContestListComponent },
              { path: 'new', component: AdminContestCreatorComponent, data: { breadcrumb: 'Create' } },
              {
                path: ':contestId', data: { breadcrumb: 'View' },
                children: [
                  { path: '', pathMatch: 'full', component: AdminContestEditorComponent },
                  {
                    path: 'registrations',
                    component: AdminContestRegistrationsComponent,
                    data: { breadcrumb: 'Registrations' }
                  }
                ]
              }
            ]
          },
          {
            path: 'problem',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator', 'ContestManager'], breadcrumb: 'Problems' },
            children: [
              { path: '', pathMatch: 'full', component: AdminProblemListComponent },
              { path: 'new', component: AdminProblemCreatorComponent, data: { breadcrumb: 'Create' } },
              {
                path: 'archive',
                component: AdminProblemArchiveComponent,
                data: { breadcrumb: 'Archive' }
              },
              {
                path: ':problemId', data: { breadcrumb: 'View' },
                children: [
                  { path: '', pathMatch: 'full', component: AdminProblemEditorComponent },
                  {
                    path: 'test-cases',
                    component: AdminProblemTestCasesComponent,
                    data: { breadcrumb: 'Test Cases' }
                  }
                ]
              }
            ]
          },
          {
            path: 'submission',
            canActivate: [AdminGuard],
            data: { roles: ['Administrator', 'SubmissionManager'], breadcrumb: 'Submissions' },
            children: [
              { path: '', pathMatch: 'full', component: AdminSubmissionListComponent },
              {
                path: 'rejudge',
                component: AdminSubmissionRejudgeComponent,
                data: { breadcrumb: 'Rejudge' }
              },
              {
                path: ':submissionId',
                component: AdminSubmissionEditorComponent,
                data: { breadcrumb: 'View' }
              }
            ]
          }
        ]
      }
    ]),
    ApiAuthorizationModule,
    MarkdownModule.forRoot(),
    NzLayoutModule,
    NzMenuModule,
    NzCardModule,
    NzPageHeaderModule,
    NzButtonModule,
    NzIconModule,
    NzFormModule,
    NzInputModule,
    NzSelectModule,
    NzDatePickerModule,
    NzCheckboxModule,
    NzTableModule,
    NzPopconfirmModule,
    NzDividerModule,
    NzTagModule,
    NzRadioModule,
    NzBreadCrumbModule,
    NzDropDownModule,
    FormsModule,
  ],
  declarations: [
    AdminComponent,
    AdminDashboardComponent,
    AdminBulletinListComponent,
    AdminBulletinFormComponent,
    AdminBulletinCreatorComponent,
    AdminBulletinEditorComponent,
    AdminUserListComponent,
    AdminUserFormComponent,
    AdminUserEditorComponent,
    AdminContestListComponent,
    AdminContestFormComponent,
    AdminContestCreatorComponent,
    AdminContestEditorComponent,
    AdminContestRegistrationsComponent,
    AdminProblemListComponent,
    AdminProblemFormComponent,
    AdminProblemCreatorComponent,
    AdminProblemArchiveComponent,
    AdminProblemEditorComponent,
    AdminProblemTestCasesComponent,
    AdminSubmissionListComponent,
    AdminSubmissionFormComponent,
    AdminSubmissionEditorComponent,
    AdminSubmissionRejudgeComponent
  ],
  exports: [
    RouterModule
  ]
})
export class AdminModule {
}