from django.contrib import admin
from django.urls import path,include
from . import views


urlpatterns = [
    path('', views.index , name='index'),
    path('books', views.books , name='books'),
    path('update', views.update , name='update'),
    path('delete', views.delete , name='delete'),
]